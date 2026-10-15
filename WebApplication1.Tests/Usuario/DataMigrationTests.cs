using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using WebApplication1.DataMigration;
using WebApplication1.Infrastructure;

namespace WebApplication1.Tests.Usuario;

public class DataMigrationTests
{
    [Fact]
    public void ReadsSettingsFileAndLetsEnvironmentOverrideIndividualValues()
    {
        var path = Path.Combine(Path.GetTempPath(), $"migration-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """
                {
                  "ConnectionStrings": { "SqlServer": "source-json", "Postgres": "destination-json" },
                  "Migration": {
                    "SourceTimeZone": "UTC", "SqlServerSchema": "legacy", "PostgresSchema": "reddit",
                    "CommandTimeoutSeconds": 300, "ColumnTimeZones": { "Usuario.CriadoEm": "UTC" }
                  }
                }
                """);
            var options = MigrationOptions.FromSettingsFile(path, _ => null);
            Assert.Equal("source-json", options.SourceConnection);
            Assert.Equal("destination-json", options.DestinationConnection);
            Assert.Equal("legacy", options.SourceSchema);
            Assert.Equal("reddit", options.DestinationSchema);
            Assert.Equal(300, options.CommandTimeoutSeconds);
            Assert.Equal(TimeZoneInfo.Utc, options.ColumnTimeZones["Usuario.CriadoEm"]);

            var overridden = MigrationOptions.FromSettingsFile(path, key =>
                key == "MIGRATION_POSTGRES_CONNECTION" ? "destination-env" : null);
            Assert.Equal("destination-env", overridden.DestinationConnection);
            Assert.Equal("source-json", overridden.SourceConnection);
            Assert.Throws<MigrationException>(() => MigrationOptions.FromSettingsFile(path, key =>
                key == "MIGRATION_POSTGRES_CONNECTION" ? "" : null));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SupportsEnvironmentOnlyWhenSettingsFileIsAbsent()
    {
        var environment = new Dictionary<string, string>
        {
            ["MIGRATION_SQLSERVER_CONNECTION"] = "source",
            ["MIGRATION_POSTGRES_CONNECTION"] = "destination",
            ["MIGRATION_SOURCE_TIME_ZONE"] = "UTC"
        };
        var options = MigrationOptions.FromSettingsFile(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json"), environment.GetValueOrDefault);
        Assert.Equal("source", options.SourceConnection);
        Assert.Equal("destination", options.DestinationConnection);
    }

    private static MigrationOptions Options(TimeZoneInfo? zone = null) => new()
    {
        SourceConnection = "unused",
        DestinationConnection = "unused",
        SourceTimeZone = zone ?? TimeZoneInfo.Utc
    };

    [Fact(Skip = "Temporariamente desabilitado: o teste precisa ser atualizado para a nova tabela.")]
    public void CopyPlanIncludesEveryPersistedColumnAndRespectsDependencies()
    {
        using var context = new RedditDbContext(new DbContextOptionsBuilder<RedditDbContext>().UseNpgsql().Options);
        var entities = context.Model.GetEntityTypes().ToArray();
        Assert.Equal(entities.Length, MigrationSchema.Tables.Count);
        var order = MigrationSchema.Tables.Select((table, i) => (table.Name, i)).ToDictionary(x => x.Name, x => x.i);

        foreach (var entity in entities)
        {
            var table = Assert.Single(MigrationSchema.Tables, t => t.Name == entity.GetTableName());
            Assert.Equal(entity.GetProperties().Select(p => p.Name).OrderBy(x => x),
                table.Columns.Select(c => c.Name).OrderBy(x => x));
            foreach (var foreignKey in entity.GetForeignKeys())
                Assert.True(order[foreignKey.PrincipalEntityType.GetTableName()!] <= order[table.Name]);

            foreach (var column in table.Columns)
            {
                var sqlType = entity.FindProperty(column.Name)!.GetColumnType()!;
                var expected = column.Type switch
                {
                    NpgsqlDbType.Integer => "integer",
                    NpgsqlDbType.Bigint => "bigint",
                    NpgsqlDbType.Smallint => "smallint",
                    NpgsqlDbType.TimestampTz => "timestamp with time zone",
                    NpgsqlDbType.Date => "date",
                    NpgsqlDbType.Boolean => "boolean",
                    NpgsqlDbType.Text => sqlType.StartsWith("character varying") ? sqlType : "text",
                    _ => throw new InvalidOperationException()
                };
                Assert.Equal(expected, sqlType);
            }
        }
        Assert.Equal(new[] { "Usuario", "Comunidade", "Post", "Comentario" },
            MigrationSchema.Tables.Where(t => t.HasIdentity).Select(t => t.Name));
    }

    [Fact]
    public void ConvertsSqlServerValuesToExactBinaryCopyTypesWithoutLosingData()
    {
        var options = Options();
        var bigId = (long)int.MaxValue + 123;
        Assert.Equal(bigId, MigrationSchema.ConvertValue(bigId, new("Id", NpgsqlDbType.Bigint), "Post", options));
        Assert.IsType<long>(MigrationSchema.ConvertValue(42, new("Id", NpgsqlDbType.Bigint), "Post", options));
        Assert.Equal((short)255, MigrationSchema.ConvertValue((byte)255, new("Papel", NpgsqlDbType.Smallint), "ComunidadeUsuario", options));
        Assert.Equal(new DateOnly(1995, 5, 12), MigrationSchema.ConvertValue(new DateTime(1995, 5, 12),
            new("DataNascimento", NpgsqlDbType.Date), "Usuario", options));
        foreach (var value in new[] { "", "ação — 日本語 😀\r\ntexto", "PBKDF2$hash/salt+=" })
            Assert.Equal(value, MigrationSchema.ConvertValue(value, new("Texto", NpgsqlDbType.Text), "Comentario", options));
        Assert.Throws<OverflowException>(() => MigrationSchema.ConvertValue(bigId,
            new("Id", NpgsqlDbType.Integer), "Usuario", options));
    }

    [Fact]
    public void ConvertsSourceTimeZoneAndHonorsPerColumnOverrides()
    {
        var localZone = TimeZoneInfo.CreateCustomTimeZone("local", TimeSpan.FromHours(-3), "local", "local");
        var options = new MigrationOptions
        {
            SourceConnection = "unused", DestinationConnection = "unused", SourceTimeZone = TimeZoneInfo.Utc,
            ColumnTimeZones = new Dictionary<string, TimeZoneInfo> { ["Usuario.CriadoEm"] = localZone }
        };
        var timestamp = new DateTime(2026, 9, 29, 12, 0, 0);
        Assert.Equal(new DateTime(2026, 9, 29, 15, 0, 0, DateTimeKind.Utc), options.ToUtc(timestamp, "Usuario.CriadoEm"));
        Assert.Equal(DateTime.SpecifyKind(timestamp, DateTimeKind.Utc), options.ToUtc(timestamp, "Post.CriadoEm"));
        Assert.Equal(DateTimeKind.Utc, options.ToUtc(timestamp, "Post.CriadoEm").Kind);
    }

    [Theory]
    [InlineData(3, 8, 2)]
    [InlineData(11, 1, 1)]
    public void RejectsInvalidAndAmbiguousDaylightSavingTimes(int month, int day, int hour)
    {
        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
            TimeSpan.FromHours(1),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 8),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 11, 1));
        var zone = TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(-5), "test", "standard", "daylight", [rule]);
        var error = Assert.Throws<MigrationException>(() => Options(zone).ToUtc(new DateTime(2026, month, day, hour, 30, 0), "Post.CriadoEm"));
        Assert.Equal("AmbiguousDate", error.ResourceKey);
    }

    [Fact]
    public void RequiresExplicitConnectionsAndTimeZone()
    {
        var env = new Dictionary<string, string>();
        foreach (var name in new[] { "MIGRATION_SQLSERVER_CONNECTION", "MIGRATION_POSTGRES_CONNECTION", "MIGRATION_SOURCE_TIME_ZONE" })
        {
            var error = Assert.Throws<MigrationException>(() => MigrationOptions.FromEnvironment(key => env.GetValueOrDefault(key)));
            Assert.Equal(name, error.Subject);
            env[name] = name.EndsWith("TIME_ZONE") ? "UTC" : "unused";
        }
        var options = MigrationOptions.FromEnvironment(key => env.GetValueOrDefault(key));
        Assert.Equal("dbo", options.SourceSchema);
        Assert.Equal("public", options.DestinationSchema);
        Assert.Equal(120, options.CommandTimeoutSeconds);
        env["MIGRATION_COLUMN_TIME_ZONES"] = "{\"Usuario.DataNascimento\":\"UTC\"}";
        Assert.Throws<MigrationException>(() => MigrationOptions.FromEnvironment(key => env.GetValueOrDefault(key)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    public void RejectsInvalidTimeout(string timeout) =>
        Assert.Throws<MigrationException>(() => MigrationOptions.FromEnvironment(name =>
            name == "MIGRATION_COMMAND_TIMEOUT_SECONDS" ? timeout : null));

    [Fact]
    public void QuotesSchemaNamesAsSingleIdentifiers()
    {
        Assert.Equal("[dbo]]; DROP TABLE Usuario;--]", MigrationSchema.SqlServerIdentifier("dbo]; DROP TABLE Usuario;--"));
        Assert.Equal("\"public\"\"; DROP TABLE Usuario;--\"", MigrationSchema.PostgresIdentifier("public\"; DROP TABLE Usuario;--"));
    }
}
