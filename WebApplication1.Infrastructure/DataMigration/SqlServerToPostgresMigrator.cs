using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace WebApplication1.DataMigration;

/// <summary>One-time, data-only import into an empty PostgreSQL Reddit schema.</summary>
public sealed class SqlServerToPostgresMigrator(MigrationOptions options)
{
    public async Task<IReadOnlyDictionary<string, long>> RunAsync(
        bool execute, Action<string, long>? progress = null, CancellationToken cancellationToken = default)
    {
        await using var source = new SqlConnection(options.SourceConnection);
        // Error details can contain email addresses, password hashes or other row values.
        var destinationSettings = new NpgsqlConnectionStringBuilder(options.DestinationConnection)
        {
            IncludeErrorDetail = false,
            LogParameters = false,
            CommandTimeout = options.CommandTimeoutSeconds
        };
        await using var destination = new NpgsqlConnection(destinationSettings.ConnectionString);
        await source.OpenAsync(cancellationToken);
        await destination.OpenAsync(cancellationToken);
        await using var sourceTransaction = (SqlTransaction)await source.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var destinationTransaction = await destination.BeginTransactionAsync(cancellationToken);

        // Lock before checking emptiness, so another writer cannot race the import.
        var tables = string.Join(", ", MigrationSchema.Tables.Select(t => DestinationTable(t.Name)));
        await ExecuteDestinationAsync($"LOCK TABLE {tables} IN SHARE ROW EXCLUSIVE MODE", destination,
            destinationTransaction, cancellationToken);

        var counts = new Dictionary<string, long>();
        foreach (var table in MigrationSchema.Tables)
        {
            await using var destinationCount = DestinationCommand(
                $"SELECT COUNT(*) FROM {DestinationTable(table.Name)}", destination, destinationTransaction);
            if ((long)(await destinationCount.ExecuteScalarAsync(cancellationToken))! != 0)
                throw new MigrationException("DestinationNotEmpty", table.Name);

            // Shared table locks are retained until the read transaction ends.
            // The source is SELECT-only and does not require snapshot isolation to be enabled.
            await using var sourceCount = SourceCommand(
                $"SELECT COUNT_BIG(*) FROM {SourceTable(table.Name)} WITH (TABLOCK, HOLDLOCK)", source, sourceTransaction);
            counts.Add(table.Name, (long)(await sourceCount.ExecuteScalarAsync(cancellationToken))!);
        }

        foreach (var table in MigrationSchema.Tables)
        {
            progress?.Invoke(table.Name, -1);
            var sourceColumns = string.Join(", ", table.Columns.Select(c => c.SourceExpression ?? MigrationSchema.SqlServerIdentifier(c.Name)));
            var destinationColumns = string.Join(", ", table.Columns.Select(c => MigrationSchema.PostgresIdentifier(c.Name)));
            await using var select = SourceCommand($"SELECT {sourceColumns} FROM {SourceTable(table.Name)}", source, sourceTransaction);
            await using (var reader = await select.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken))
            await using (var writer = await destination.BeginBinaryImportAsync(
                             $"COPY {DestinationTable(table.Name)} ({destinationColumns}) FROM STDIN (FORMAT BINARY)", cancellationToken))
            {
                writer.Timeout = TimeSpan.FromSeconds(options.CommandTimeoutSeconds);
                long copied = 0;
                while (await reader.ReadAsync(cancellationToken))
                {
                    await writer.StartRowAsync(cancellationToken);
                    for (var i = 0; i < table.Columns.Length; i++)
                    {
                        if (await reader.IsDBNullAsync(i, cancellationToken))
                            await writer.WriteNullAsync(cancellationToken);
                        else
                        {
                            var column = table.Columns[i];
                            var value = MigrationSchema.ConvertValue(reader.GetValue(i), column, table.Name, options);
                            await writer.WriteAsync(value, column.Type, cancellationToken);
                        }
                    }
                    copied++;
                }

                var imported = await writer.CompleteAsync(cancellationToken);
                if (copied != counts[table.Name] || imported != (ulong)copied)
                    throw new MigrationException("CountMismatch", table.Name);
            }
            progress?.Invoke(table.Name, counts[table.Name]);
        }

        // Validate persisted counts, including effects of any destination triggers.
        foreach (var table in MigrationSchema.Tables)
        {
            await using var count = DestinationCommand($"SELECT COUNT(*) FROM {DestinationTable(table.Name)}",
                destination, destinationTransaction);
            if ((long)(await count.ExecuteScalarAsync(cancellationToken))! != counts[table.Name])
                throw new MigrationException("CountMismatch", table.Name);

            if (table.HasIdentity)
            {
                await using var max = DestinationCommand($"SELECT MAX(\"Id\") FROM {DestinationTable(table.Name)}",
                    destination, destinationTransaction);
                var value = await max.ExecuteScalarAsync(cancellationToken);
                var next = value is null or DBNull ? 1 : checked(Math.Max(0, Convert.ToInt64(value)) + 1);
                // Unlike setval(), RESTART participates in rollback, including dry runs.
                await ExecuteDestinationAsync(
                    $"ALTER TABLE {DestinationTable(table.Name)} ALTER COLUMN \"Id\" RESTART WITH {next.ToString(CultureInfo.InvariantCulture)}",
                    destination, destinationTransaction, cancellationToken);
            }
        }

        await sourceTransaction.CommitAsync(cancellationToken);
        if (execute)
            await destinationTransaction.CommitAsync(cancellationToken);
        else
            await destinationTransaction.RollbackAsync(cancellationToken);
        return counts;
    }

    private string SourceTable(string table) =>
        $"{MigrationSchema.SqlServerIdentifier(options.SourceSchema)}.{MigrationSchema.SqlServerIdentifier(table)}";

    private string DestinationTable(string table) =>
        $"{MigrationSchema.PostgresIdentifier(options.DestinationSchema)}.{MigrationSchema.PostgresIdentifier(table)}";

    private SqlCommand SourceCommand(string sql, SqlConnection connection, SqlTransaction transaction) =>
        new(sql, connection, transaction) { CommandTimeout = options.CommandTimeoutSeconds };

    private NpgsqlCommand DestinationCommand(string sql, NpgsqlConnection connection, NpgsqlTransaction transaction) =>
        new(sql, connection, transaction) { CommandTimeout = options.CommandTimeoutSeconds };

    private async Task ExecuteDestinationAsync(string sql, NpgsqlConnection connection,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        await using var command = DestinationCommand(sql, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
