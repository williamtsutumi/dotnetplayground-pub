using System.Globalization;
using System.Text.Json;

namespace WebApplication1.DataMigration;

public sealed class MigrationOptions
{
    public required string SourceConnection { get; init; }
    public required string DestinationConnection { get; init; }
    public string SourceSchema { get; init; } = "dbo";
    public string DestinationSchema { get; init; } = "public";
    public required TimeZoneInfo SourceTimeZone { get; init; }
    public IReadOnlyDictionary<string, TimeZoneInfo> ColumnTimeZones { get; init; }
        = new Dictionary<string, TimeZoneInfo>();
    public int CommandTimeoutSeconds { get; init; } = 120;

    public static MigrationOptions FromSettingsFile(string path, Func<string, string?> readEnvironment)
    {
        // Resolve relative to the executable, independently of the working directory.
        using var settings = File.Exists(path)
            ? JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            })
            : null;
        var keys = new Dictionary<string, string[]>
        {
            ["MIGRATION_SQLSERVER_CONNECTION"] = ["ConnectionStrings", "SqlServer"],
            ["MIGRATION_POSTGRES_CONNECTION"] = ["ConnectionStrings", "Postgres"],
            ["MIGRATION_POSTGRES_SOURCE_CONNECTION"] = ["ConnectionStrings", "PostgresSource"],
            ["MIGRATION_SOURCE_TIME_ZONE"] = ["Migration", "SourceTimeZone"],
            ["MIGRATION_SQLSERVER_SCHEMA"] = ["Migration", "SqlServerSchema"],
            ["MIGRATION_POSTGRES_SCHEMA"] = ["Migration", "PostgresSchema"],
            ["MIGRATION_COMMAND_TIMEOUT_SECONDS"] = ["Migration", "CommandTimeoutSeconds"],
            ["MIGRATION_COLUMN_TIME_ZONES"] = ["Migration", "ColumnTimeZones"]
        };

        string? Read(string name)
        {
            var environmentValue = readEnvironment(name);
            if (environmentValue is not null) return environmentValue;
            if (settings is null) return null;
            var value = settings.RootElement;
            foreach (var key in keys[name])
                if (!value.TryGetProperty(key, out value)) return null;
            return value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => value.GetString(),
                _ => value.GetRawText()
            };
        }

        try { return FromEnvironment(Read); }
        catch (MigrationException ex) when (ex.ResourceKey is "MissingSetting" or "InvalidSetting")
        {
            throw new MigrationException(ex.ResourceKey,
                $"{string.Join(":", keys[ex.Subject])} / {ex.Subject}");
        }
    }

    public static MigrationOptions FromEnvironment(Func<string, string?> read)
    {
        string Required(string name) => string.IsNullOrWhiteSpace(read(name))
            ? throw new MigrationException("MissingSetting", name) : read(name)!;

        var timeout = read("MIGRATION_COMMAND_TIMEOUT_SECONDS") ?? "120";
        if (!int.TryParse(timeout, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) || seconds < 1)
            throw new MigrationException("InvalidSetting", "MIGRATION_COMMAND_TIMEOUT_SECONDS");

        var zones = new Dictionary<string, TimeZoneInfo>();
        foreach (var pair in JsonSerializer.Deserialize<Dictionary<string, string>>(
                     read("MIGRATION_COLUMN_TIME_ZONES") ?? "{}") ?? new())
        {
            if (!MigrationSchema.Tables.Any(t => t.Columns.Any(c =>
                    $"{t.Name}.{c.Name}" == pair.Key && c.Type == NpgsqlTypes.NpgsqlDbType.TimestampTz)))
                throw new MigrationException("InvalidSetting", "MIGRATION_COLUMN_TIME_ZONES");
            zones.Add(pair.Key, TimeZoneInfo.FindSystemTimeZoneById(pair.Value));
        }

        return new MigrationOptions
        {
            SourceConnection = Required("MIGRATION_SQLSERVER_CONNECTION"),
            DestinationConnection = Required("MIGRATION_POSTGRES_CONNECTION"),
            SourceSchema = read("MIGRATION_SQLSERVER_SCHEMA") ?? "dbo",
            DestinationSchema = read("MIGRATION_POSTGRES_SCHEMA") ?? "public",
            SourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(Required("MIGRATION_SOURCE_TIME_ZONE")),
            ColumnTimeZones = zones,
            CommandTimeoutSeconds = seconds
        };
    }

    public static MigrationOptions FromPostgresSettingsFile(string path, Func<string, string?> readEnvironment)
    {
        using var settings = File.Exists(path)
            ? JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            })
            : null;
        var keys = new Dictionary<string, string[]>
        {
            ["MIGRATION_POSTGRES_SOURCE_CONNECTION"] = ["ConnectionStrings", "PostgresSource"],
            ["MIGRATION_POSTGRES_CONNECTION"] = ["ConnectionStrings", "Postgres"],
            ["MIGRATION_POSTGRES_SOURCE_SCHEMA"] = ["Migration", "PostgresSourceSchema"],
            ["MIGRATION_POSTGRES_SCHEMA"] = ["Migration", "PostgresSchema"],
            ["MIGRATION_COMMAND_TIMEOUT_SECONDS"] = ["Migration", "CommandTimeoutSeconds"]
        };

        string? Read(string name)
        {
            var environmentValue = readEnvironment(name);
            if (environmentValue is not null) return environmentValue;
            if (settings is null) return null;
            var value = settings.RootElement;
            foreach (var key in keys[name])
                if (!value.TryGetProperty(key, out value)) return null;
            return value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => value.GetString(),
                _ => value.GetRawText()
            };
        }

        try { return FromPostgresEnvironment(Read); }
        catch (MigrationException ex) when (ex.ResourceKey is "MissingSetting" or "InvalidSetting")
        {
            throw new MigrationException(ex.ResourceKey,
                $"{string.Join(":", keys[ex.Subject])} / {ex.Subject}");
        }
    }

    public static MigrationOptions FromPostgresEnvironment(Func<string, string?> read)
    {
        string Required(string name) => string.IsNullOrWhiteSpace(read(name))
            ? throw new MigrationException("MissingSetting", name) : read(name)!;

        var timeout = read("MIGRATION_COMMAND_TIMEOUT_SECONDS") ?? "120";
        if (!int.TryParse(timeout, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) || seconds < 1)
            throw new MigrationException("InvalidSetting", "MIGRATION_COMMAND_TIMEOUT_SECONDS");

        return new MigrationOptions
        {
            SourceConnection = Required("MIGRATION_POSTGRES_SOURCE_CONNECTION"),
            DestinationConnection = Required("MIGRATION_POSTGRES_CONNECTION"),
            SourceSchema = read("MIGRATION_POSTGRES_SOURCE_SCHEMA") ?? "public",
            DestinationSchema = read("MIGRATION_POSTGRES_SCHEMA") ?? "public",
            SourceTimeZone = TimeZoneInfo.Utc,
            CommandTimeoutSeconds = seconds
        };
    }

    public DateTime ToUtc(DateTime value, string column)
    {
        var zone = ColumnTimeZones.GetValueOrDefault(column, SourceTimeZone);
        var unspecified = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(unspecified) || zone.IsAmbiguousTime(unspecified))
            throw new MigrationException("AmbiguousDate", column);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, zone);
    }
}

public sealed class MigrationException(string resourceKey, string subject) : Exception(resourceKey)
{
    public string ResourceKey { get; } = resourceKey;
    public string Subject { get; } = subject;
}
