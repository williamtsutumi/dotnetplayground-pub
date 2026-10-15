# Copiar dados para PostgreSQL

Console independente dentro de Infrastructure. As dependências de SQL Server
ficam neste utilitário e não são adicionadas à aplicação Web.

## Preparação

- Aplique `InitialPostgreSql` separadamente no destino, conforme
  [as instruções das migrações](../Migrations/README.md). O programa copia somente
  dados; não cria tabelas nem copia `__EFMigrationsHistory`.
- As sete tabelas do destino devem estar vazias. Se qualquer uma contiver dados,
  o programa aborta sem apagar, substituir ou mesclar registros.
- A origem precisa ter as colunas do modelo atual, incluindo `Post.ComunidadeId`,
  `Post.ConteudoMarkdown`, `Usuario.IdiomaPreferido`, `Senha` e `DataNascimento`.
- Pare as gravações da aplicação antes da migração e mantenha-as paradas até
  trocar a conexão para o PostgreSQL. Não há sincronização de alterações posteriores.
- A conexão SQL Server precisa de SELECT nas sete tabelas. Use uma conta somente
  de leitura. A conexão PostgreSQL precisa de SELECT, INSERT e permissão de dono
  das tabelas para reiniciar suas identidades. Ambos os bancos devem usar os
  tipos e relacionamentos previstos no projeto.

## Execução na raiz do repositório (PowerShell)

Preencha `DataMigration/appsettings.json` no projeto Infrastructure:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=Reddit;User Id=seu_usuario;Password=sua_senha",
    "Postgres": "Host=localhost;Port=5432;Database=Reddit;Username=postgres;Password=sua_senha"
  },
  "Migration": {
    "SourceTimeZone": "UTC",
    "SqlServerSchema": "dbo",
    "PostgresSchema": "public",
    "CommandTimeoutSeconds": 120,
    "ColumnTimeZones": {}
  }
}
```

Use `UTC` apenas se os horários da origem representam UTC; veja a seção de datas
abaixo. O arquivo local `appsettings.json` é ignorado pelo Git. Em um checkout
novo, copie `appsettings.example.json` para `appsettings.json` e preencha os valores.
O build copia o arquivo para junto do executável; a leitura independe da pasta
atual do terminal. Os arquivos de saída/publicação também conterão essas conexões.
O utilitário não lê o `appsettings.json` da aplicação Web.

```powershell
dotnet run --project WebApplication1.Infrastructure/DataMigration/WebApplication1.DataMigration.csproj -- --dry-run
# Após uma simulação bem-sucedida:
dotnet run --project WebApplication1.Infrastructure/DataMigration/WebApplication1.DataMigration.csproj -- --execute
```

Para copiar de uma origem PostgreSQL, use `--postgres-to-postgres`. Configure
`ConnectionStrings:PostgresSource` e `ConnectionStrings:Postgres` no JSON, ou as
variáveis `MIGRATION_POSTGRES_SOURCE_CONNECTION` e `MIGRATION_POSTGRES_CONNECTION`.
Os schemas padrão são `public`; personalize `Migration:PostgresSourceSchema` /
`MIGRATION_POSTGRES_SOURCE_SCHEMA` e `Migration:PostgresSchema` /
`MIGRATION_POSTGRES_SCHEMA` conforme necessário. A origem usa uma transação
`REPEATABLE READ` com locks de leitura para manter contagens e dados consistentes.

```powershell
dotnet run --project WebApplication1.Infrastructure/DataMigration/WebApplication1.DataMigration.csproj -- --postgres-to-postgres --dry-run
dotnet run --project WebApplication1.Infrastructure/DataMigration/WebApplication1.DataMigration.csproj -- --postgres-to-postgres --execute
```

### Alternativa: variáveis de ambiente

As variáveis abaixo têm prioridade sobre o arquivo, inclusive quando estão vazias.
Remova variáveis antigas da sessão para voltar a usar os valores do JSON.

```powershell
# Forneça as conexões sem armazená-las em arquivos versionados.
# Exemplos de formatos:
# SQL Server: Server=localhost;Database=Reddit;Integrated Security=True;Encrypt=True
# PostgreSQL: Host=localhost;Port=5432;Database=Reddit;Username=postgres;Password=...
$env:MIGRATION_SQLSERVER_CONNECTION = Read-Host 'Conexão SQL Server' -MaskInput
$env:MIGRATION_POSTGRES_CONNECTION = Read-Host 'Conexão PostgreSQL de destino' -MaskInput
$env:MIGRATION_POSTGRES_SOURCE_CONNECTION = Read-Host 'Conexão PostgreSQL de origem' -MaskInput

# Obrigatório: escolha o fuso que os valores datetime da origem representam.
$env:MIGRATION_SOURCE_TIME_ZONE = 'UTC'

# Primeiro execute a cópia completa com validação e rollback.
dotnet run --project WebApplication1.Infrastructure/DataMigration/WebApplication1.DataMigration.csproj -- --dry-run

# Depois confirme a cópia definitiva.
dotnet run --project WebApplication1.Infrastructure/DataMigration/WebApplication1.DataMigration.csproj -- --execute

Remove-Item Env:MIGRATION_SQLSERVER_CONNECTION, Env:MIGRATION_POSTGRES_CONNECTION, Env:MIGRATION_POSTGRES_SOURCE_CONNECTION
```

`-MaskInput` requer PowerShell 7.1 ou superior. Sem argumentos, ou com `--help`,
o programa mostra ajuda e não abre conexões. Códigos de saída: 0 sucesso, 1 falha,
2 argumentos inválidos, 130 cancelamento com Ctrl+C.

## Datas e opções

`datetime` do SQL Server não carrega fuso. Use `UTC` somente para valores que já
representam UTC. Para horários locais, use por exemplo `America/Sao_Paulo`.
Os horários são convertidos para UTC; datas de nascimento permanecem datas civis.
Horários ambíguos ou inexistentes em transições de horário de verão abortam a cópia.

Se colunas antigas usavam `GETDATE()` e outras `UtcNow`, configure cada exceção
ao fuso geral. Exemplo (ajuste conforme a origem real dos seus dados):

```powershell
$env:MIGRATION_COLUMN_TIME_ZONES = '{"Usuario.CriadoEm":"America/Sao_Paulo","ComunidadeUsuario.EntrouEm":"America/Sao_Paulo"}'
```

Uma mesma coluna com mistura de UTC e horário local requer correção na origem
antes da migração; o programa não consegue inferir o fuso de cada registro.

No JSON, configure as mesmas exceções como objeto em `Migration:ColumnTimeZones`,
por exemplo `"ColumnTimeZones": { "Usuario.CriadoEm": "America/Sao_Paulo" }`.

| Variável opcional | Padrão |
| --- | --- |
| `MIGRATION_SQLSERVER_SCHEMA` | `dbo` |
| `MIGRATION_POSTGRES_SCHEMA` | `public` |
| `MIGRATION_POSTGRES_SOURCE_SCHEMA` | `public` (modo PostgreSQL → PostgreSQL) |
| `MIGRATION_COMMAND_TIMEOUT_SECONDS` | `120` (inteiro positivo) |
| `MIGRATION_COLUMN_TIME_ZONES` | `{}` (objeto JSON com `Tabela.Coluna` → fuso) |
| `MIGRATION_LANGUAGE` | português; use `en` para inglês |

## Comportamento

A origem é lida em transação serializable com locks compartilhados nas tabelas.
O destino também é bloqueado contra gravações concorrentes, antes de verificar
que está vazio. O programa copia Usuario, Comunidade, ComunidadeUsuario, Post,
Comentario, UsuarioUpvotePost e UsuarioUpvoteComentario nessa ordem.

Cada tabela é enviada por um único COPY binário, preservando IDs, referências,
NULLs, textos Unicode e hashes de senha. Todos os comentários são enviados na
mesma instrução, permitindo referências a pais independentemente da ordem dos IDs.
As constraints continuam ativas. Nenhum hash de senha é recalculado.

As contagens da origem, da cópia e do destino precisam coincidir. Os próximos
IDs são reiniciados em `MAX(Id) + 1` (no mínimo 1). O ajuste usa RESTART, que
participa da transação; uma simulação desfaz os dados e o ajuste das identidades.
Não se usa `setval`, cujo efeito não seria desfeito no rollback.

O destino é confirmado apenas ao concluir todas as tabelas e validações. Falhas
antes do commit desfazem a transação. Se a conexão cair durante o commit, confira
o destino: o servidor pode ter confirmado sem o cliente receber a resposta.
Uma nova execução recusa destino preenchido. Logs não exibem linhas, senhas ou
conexões; o código de erro e a última tabela anunciada ajudam no diagnóstico.

Referências: [COPY no Npgsql](https://www.npgsql.org/doc/copy.html) e
[RESTART transacional no PostgreSQL](https://www.postgresql.org/docs/current/sql-altersequence.html).
