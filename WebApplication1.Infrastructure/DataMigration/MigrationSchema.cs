using NpgsqlTypes;

namespace WebApplication1.DataMigration;

public sealed record MigrationColumn(string Name, NpgsqlDbType Type, string? SourceExpression = null);
public sealed record MigrationTable(string Name, bool HasIdentity, params MigrationColumn[] Columns);

public static class MigrationSchema
{
    private static MigrationColumn Int(string name) => new(name, NpgsqlDbType.Integer);
    private static MigrationColumn BigInt(string name) => new(name, NpgsqlDbType.Bigint);
    private static MigrationColumn Text(string name) => new(name, NpgsqlDbType.Text);
    private static MigrationColumn Timestamp(string name) => new(name, NpgsqlDbType.TimestampTz);

    // Dependency order; all comments are inserted in one statement, including parent references.
    public static IReadOnlyList<MigrationTable> Tables { get; } = new MigrationTable[]
    {
        new("Usuario", true, Int("Id"), Text("NomeUsuario"), Text("Email"), Text("IdiomaPreferido"),
            Timestamp("CriadoEm"), Text("Senha"), new("DataNascimento", NpgsqlDbType.Date),
            new("Descricao", NpgsqlDbType.Text, "CAST(NULL AS varchar(1000))")),
        new("Comunidade", true, Int("Id"), Text("Nome"), Text("Descricao"), Int("IdCriadorUsuario"), Timestamp("CriadoEm")),
        new("ComunidadeUsuario", false, Int("IdComunidade"), Int("IdUsuario"), new("Papel", NpgsqlDbType.Smallint), Timestamp("EntrouEm")),
        new("Post", true, BigInt("Id"), Int("ComunidadeId"), Int("IdAutorUsuario"), Text("Titulo"), Text("Conteudo"),
            new("ConteudoMarkdown", NpgsqlDbType.Boolean), Timestamp("CriadoEm"), Timestamp("EditadoEm")),
        new("Comentario", true, Int("Id"), BigInt("IdPost"), Int("IdComentario"), Int("IdAutorUsuario"), Text("Texto"), Timestamp("CriadoEm")),
        new("UsuarioUpvotePost", false, Int("IdUsuario"), BigInt("IdPost"), Int("Tipo")),
        new("UsuarioUpvoteComentario", false, Int("IdUsuario"), Int("IdComentario"), Int("Tipo"))
    };

    public static string SqlServerIdentifier(string value) => "[" + value.Replace("]", "]]") + "]";
    public static string PostgresIdentifier(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    public static object ConvertValue(object value, MigrationColumn column, string table, MigrationOptions options) => column.Type switch
    {
        NpgsqlDbType.Integer => Convert.ToInt32(value),
        NpgsqlDbType.Bigint => Convert.ToInt64(value),
        NpgsqlDbType.Smallint => Convert.ToInt16(value),
        NpgsqlDbType.Boolean => Convert.ToBoolean(value),
        NpgsqlDbType.TimestampTz => options.ToUtc((DateTime)value, $"{table}.{column.Name}"),
        NpgsqlDbType.Date => DateOnly.FromDateTime((DateTime)value),
        NpgsqlDbType.Text => (string)value,
        _ => throw new ArgumentOutOfRangeException(nameof(column))
    };

    public static object ConvertPostgresValue(object value, MigrationColumn column) => column.Type switch
    {
        NpgsqlDbType.Integer => Convert.ToInt32(value),
        NpgsqlDbType.Bigint => Convert.ToInt64(value),
        NpgsqlDbType.Smallint => Convert.ToInt16(value),
        NpgsqlDbType.Boolean => Convert.ToBoolean(value),
        NpgsqlDbType.TimestampTz => DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc),
        NpgsqlDbType.Date => value is DateOnly date ? date : DateOnly.FromDateTime((DateTime)value),
        NpgsqlDbType.Text => (string)value,
        _ => throw new ArgumentOutOfRangeException(nameof(column))
    };
}
