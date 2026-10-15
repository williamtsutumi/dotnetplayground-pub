using Microsoft.Data.SqlClient;
using Npgsql;
using WebApplication1.DataMigration;

if (args.Length == 0 || args is ["--help"])
{
    Console.WriteLine(Messages.Get("Help"));
    return 0;
}

var postgresToPostgres = args.Length > 0 && args[0] == "--postgres-to-postgres";
var operationArgs = postgresToPostgres ? args[1..] : args;
if (operationArgs is not ["--dry-run"] && operationArgs is not ["--execute"])
{
    Console.Error.WriteLine(Messages.Get("Help"));
    return 2;
}

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
try
{
    var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    var options = postgresToPostgres
        ? MigrationOptions.FromPostgresSettingsFile(settingsPath, Environment.GetEnvironmentVariable)
        : MigrationOptions.FromSettingsFile(settingsPath, Environment.GetEnvironmentVariable);
    var destinationDatabase = new NpgsqlConnectionStringBuilder(options.DestinationConnection).Database;
    if (string.Equals(destinationDatabase, "prod_reddit", StringComparison.OrdinalIgnoreCase))
        throw new MigrationException("ProtectedDestination", "prod_reddit");

    var execute = operationArgs[0] == "--execute";
    Console.WriteLine(Messages.Get(execute ? "StartingExecute" : "StartingDryRun"));
    var reportProgress = (string table, long count) =>
        Console.WriteLine(Messages.Get(count < 0 ? "Copying" : "Copied", table, count));
    var counts = postgresToPostgres
        ? await new PostgresToPostgresMigrator(options).RunAsync(execute, reportProgress, cancellation.Token)
        : await new SqlServerToPostgresMigrator(options).RunAsync(execute, reportProgress, cancellation.Token);
    Console.WriteLine(Messages.Get(execute ? "Committed" : "RolledBack", counts.Values.Sum()));
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine(Messages.Get("Cancelled"));
    return 130;
}
catch (MigrationException ex)
{
    Console.Error.WriteLine(Messages.Get(ex.ResourceKey, ex.Subject));
    return 1;
}
catch (Exception ex)
{
    // Never print exception messages or connection strings: server errors can contain row data.
    var code = ex switch
    {
        PostgresException postgres => postgres.SqlState,
        SqlException sql => sql.Number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        _ => ex.GetType().Name
    };
    Console.Error.WriteLine(Messages.Get("Failed", code));
    return 1;
}
