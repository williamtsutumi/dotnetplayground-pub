using System.Data;
using System.Globalization;
using Npgsql;

namespace WebApplication1.DataMigration;

/// <summary>One-time, data-only copy between two PostgreSQL Reddit schemas.</summary>
public sealed class PostgresToPostgresMigrator(MigrationOptions options)
{
    public async Task<IReadOnlyDictionary<string, long>> RunAsync(
        bool execute, Action<string, long>? progress = null, CancellationToken cancellationToken = default)
    {
        var sourceSettings = new NpgsqlConnectionStringBuilder(options.SourceConnection)
        {
            IncludeErrorDetail = false, LogParameters = false,
            CommandTimeout = options.CommandTimeoutSeconds
        };
        var destinationSettings = new NpgsqlConnectionStringBuilder(options.DestinationConnection)
        {
            IncludeErrorDetail = false, LogParameters = false,
            CommandTimeout = options.CommandTimeoutSeconds
        };
        await using var source = new NpgsqlConnection(sourceSettings.ConnectionString);
        await using var destination = new NpgsqlConnection(destinationSettings.ConnectionString);
        await source.OpenAsync(cancellationToken);
        await destination.OpenAsync(cancellationToken);
        await using var sourceTransaction = await source.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        await using var destinationTransaction = await destination.BeginTransactionAsync(cancellationToken);

        // Hold read locks so the counts and copied rows describe the same source data.
        var sourceTables = string.Join(", ", MigrationSchema.Tables.Select(t => SourceTable(t.Name)));
        await ExecuteAsync($"LOCK TABLE {sourceTables} IN SHARE MODE", source, sourceTransaction, cancellationToken);

        // Prevent writes between emptiness checks and commit.
        var destinationTables = string.Join(", ", MigrationSchema.Tables.Select(t => DestinationTable(t.Name)));
        await ExecuteAsync($"LOCK TABLE {destinationTables} IN SHARE ROW EXCLUSIVE MODE", destination,
            destinationTransaction, cancellationToken);

        var counts = new Dictionary<string, long>();
        foreach (var table in MigrationSchema.Tables)
        {
            await using var destinationCount = Command($"SELECT COUNT(*) FROM {DestinationTable(table.Name)}", destination, destinationTransaction);
            if ((long)(await destinationCount.ExecuteScalarAsync(cancellationToken))! != 0)
                throw new MigrationException("DestinationNotEmpty", table.Name);
            await using var sourceCount = Command($"SELECT COUNT(*) FROM {SourceTable(table.Name)}", source, sourceTransaction);
            counts.Add(table.Name, (long)(await sourceCount.ExecuteScalarAsync(cancellationToken))!);
        }

        foreach (var table in MigrationSchema.Tables)
        {
            progress?.Invoke(table.Name, -1);
            var columns = string.Join(", ", table.Columns.Select(c => MigrationSchema.PostgresIdentifier(c.Name)));
            await using var select = Command($"SELECT {columns} FROM {SourceTable(table.Name)}", source, sourceTransaction);
            await using var reader = await select.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            await using var writer = await destination.BeginBinaryImportAsync(
                $"COPY {DestinationTable(table.Name)} ({columns}) FROM STDIN (FORMAT BINARY)", cancellationToken);
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
                        await writer.WriteAsync(MigrationSchema.ConvertPostgresValue(reader.GetValue(i), column), column.Type, cancellationToken);
                    }
                }
                copied++;
            }
            var imported = await writer.CompleteAsync(cancellationToken);
            if (copied != counts[table.Name] || imported != (ulong)copied)
                throw new MigrationException("CountMismatch", table.Name);
            progress?.Invoke(table.Name, copied);
        }

        foreach (var table in MigrationSchema.Tables)
        {
            await using var count = Command($"SELECT COUNT(*) FROM {DestinationTable(table.Name)}", destination, destinationTransaction);
            if ((long)(await count.ExecuteScalarAsync(cancellationToken))! != counts[table.Name])
                throw new MigrationException("CountMismatch", table.Name);
            if (table.HasIdentity)
            {
                await using var max = Command($"SELECT MAX(\"Id\") FROM {DestinationTable(table.Name)}", destination, destinationTransaction);
                var value = await max.ExecuteScalarAsync(cancellationToken);
                var next = value is null or DBNull ? 1 : checked(Math.Max(0, Convert.ToInt64(value)) + 1);
                await ExecuteAsync($"ALTER TABLE {DestinationTable(table.Name)} ALTER COLUMN \"Id\" RESTART WITH {next.ToString(CultureInfo.InvariantCulture)}",
                    destination, destinationTransaction, cancellationToken);
            }
        }

        await sourceTransaction.CommitAsync(cancellationToken);
        if (execute) await destinationTransaction.CommitAsync(cancellationToken);
        else await destinationTransaction.RollbackAsync(cancellationToken);
        return counts;
    }

    private string SourceTable(string table) =>
        $"{MigrationSchema.PostgresIdentifier(options.SourceSchema)}.{MigrationSchema.PostgresIdentifier(table)}";
    private string DestinationTable(string table) =>
        $"{MigrationSchema.PostgresIdentifier(options.DestinationSchema)}.{MigrationSchema.PostgresIdentifier(table)}";
    private NpgsqlCommand Command(string sql, NpgsqlConnection connection, NpgsqlTransaction transaction) =>
        new(sql, connection, transaction) { CommandTimeout = options.CommandTimeoutSeconds };
    private async Task ExecuteAsync(string sql, NpgsqlConnection connection, NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = Command(sql, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
