# .NET Playground

## Visão geral

Projeto pessoal de estudo em C# e ASP.NET Core MVC (.NET 8), inspirado no Reddit.
Permite criar contas, participar de comunidades, publicar posts, comentar, responder
a comentários e votar em posts. Não é um produto oficial do Reddit.

A interface usa Razor, Bootstrap, CSS e JavaScript, com suporte a português brasileiro
e inglês. O banco é SQL Server, acessado pelo Entity Framework Core.
O projeto também explora publicação em AWS EC2 com Nginx, systemd, Docker e Jenkins.

## Estrutura

- `WebApplication1.Domain/`: entidades e tipos do domínio, como Usuario, Comunidade,
  Post, Comentario e os vínculos de participação e votação.
- `WebApplication1.Application/`: serviços, comandos e contratos de repositórios.
  - `Commands/`: dados de entrada dos casos de uso, agrupados por funcionalidade.
- `WebApplication1.Infrastructure/`: acesso ao banco, repositórios, contextos EF Core,
  configurações das entidades e migrações.
- `WebApplication1.Web/`: aplicação MVC e ponto de entrada em `Program.cs`.
  - `Controllers/`: rotas e tratamento das requisições.
  - `Views/`: páginas Razor, layouts e componentes compartilhados.
  - `Models/`: modelos de apresentação das telas.
  - `DTOs/`: contratos de transporte de dados da camada Web.
  - `Mappers/`: mapeamentos entre DTOs, Commands e modelos, incluindo profiles do AutoMapper.
  - `Program.cs`: composição da aplicação e registro das dependências no contêiner de DI.
  - `Resources/`: traduções e suporte à localização.
  - `Filters/`, `TagHelpers/` e `ViewComponents/`: comportamentos compartilhados da UI.
  - `wwwroot/`: estilos, scripts e outros arquivos estáticos.
  - `Publish/`: configurações e scripts de publicação.
- `WebApplication1.Tests/`: testes com xUnit.
- `Jenkins/`, `.github/` e `scripts/`: automação e apoio ao desenvolvimento/publicação.
- `README.md`: contexto do projeto e arquitetura de hospedagem.
- `.build-verification/`: ferramentas e saídas locais de verificação; não é código da aplicação.

## Telas principais

As views ficam em `WebApplication1.Web/Views/`, agrupadas por funcionalidade.

| Tela / rota | View | Conteúdo principal |
| --- | --- | --- |
| `/` ou `/Home/Index` | `Home/Index.cshtml` | Feed, ordenação, paginação e cards de posts. |
| `/Usuario/Login` e `/Usuario/Criar` | `Usuario/Login.cshtml` e `Criar.cshtml` | Login e cadastro. |
| `/Comunidade/Index` | `Comunidade/Index.cshtml` | Busca e listagem de comunidades. |
| `/Comunidade/View/{id}` | `Comunidade/Principal.cshtml` | Posts, participação, membros e informações da comunidade. |
| `/Comunidade/Criar`, `/Editar/{id}` e `/Excluir/{id}` (sob `/Comunidade`) | `Comunidade/Criar.cshtml`, `Editar.cshtml` e `Excluir.cshtml` | Criação, edição e confirmação de exclusão. |
| `/Post/Criar` e `/Comunidade/Postar/{id}` | `Post/Criar.cshtml` e `Comunidade/Postar.cshtml` | Publicação de posts, com comunidade opcional no formulário geral. |
| `/Post/View/{id}` | `Post/Detalhes.cshtml` | Post completo, novo comentário e respostas em níveis. |
| `/Perfil/Index/{id?}` | `Perfil/Index.cshtml` | Perfil próprio ou de outro usuário. |
| `/About`, `/AboutAuth`, `/Jenkins` e `/Pagination` | Pastas correspondentes em `Views/` | Páginas informativas e demonstrações do projeto. |

### Layouts e componentes compartilhados

- `_LayoutCommunity`: navbar e estrutura visual das telas sociais.
- `_CommunitySidebar`: navegação lateral; `SidebarComunidades` lista as comunidades do usuário.
- `_LayoutComunidade`: utiliza `_LayoutCommunity` e envolve formulários de comunidades.
- `_LayoutLoggedOut`: estrutura das telas de login e cadastro.
- `_Layout`: estrutura das páginas informativas e demonstrações.
- `_LanguageDropdown`: seletor PT-BR/EN nas navbars dos layouts.
- `_FeedPost`, `_PostVotes` e `_CriadoEm`: card, votação/contador de comentários e data relativa.
- `Comunidade/_Formulario`: campos compartilhados entre criação e edição de comunidades.
- `Post/Detalhes.cshtml`: comentários e respostas, com conexões visuais em `comment-threads.js`.

## Padrões de programação e responsabilidades

- Priorize o **SRP (Princípio da Responsabilidade Única)**: cada classe deve ter
  uma responsabilidade clara e um único motivo para mudar. Separe comunicação HTTP,
  orquestração de casos de uso, regras de negócio, persistência e apresentação.
- O fluxo deve seguir `Front-end → Controller → Service → Repository`.
- **Controllers** fazem somente a comunicação entre o front-end e os serviços:
  recebem e validam a estrutura da requisição, encaminham os dados ao serviço e
  convertem o resultado em view, JSON, redirecionamento ou status HTTP.
  Não implementam regras de negócio, não acessam repositories diretamente e não
  utilizam `DbContext`, consultas EF Core, SQL ou `SaveChanges`.
- **Services** são responsáveis por orquestrar o fluxo de execução dos casos de uso
  e aplicar as regras de negócio, reutilizando comportamentos das entidades de domínio
  quando apropriado. Obtêm e persistem dados exclusivamente por contratos de repositories.
  Não dependem de controllers, views, `HttpContext`, `ModelState` ou `TempData`.
- **Repositories** concentram todo acesso ao banco: consultas, buscas, listagens,
  verificações de existência, contagens, inclusões, alterações e exclusões, incluindo
  `SaveChanges`/`SaveChangesAsync`. Não transfira consultas pendentes (`IQueryable`)
  para controllers ou services; execute-as no repository e retorne os resultados.
  Repositories não orquestram casos de uso nem produzem mensagens de interface.
- Mantenha os services e contratos de repositories em `WebApplication1.Application`
  e as implementações de persistência em `WebApplication1.Infrastructure`.
- **DTOs** (`WebApplication1.Web/DTOs/`) representam os dados recebidos ou enviados
  pela camada Web. Exponha somente os campos necessários à operação e mantenha neles
  as validações de formato da entrada. Não utilize entidades persistidas diretamente
  como contratos de entrada e não coloque regras de negócio ou acesso ao banco nos DTOs.
- **Commands** (`WebApplication1.Application/Commands/`) representam a intenção e os
  dados necessários para executar um caso de uso, como criar uma comunidade ou salvar
  um usuário. Os services recebem esses contratos, sem depender de DTOs da camada Web.
  Commands transportam dados; a execução e as regras de negócio pertencem aos services.
- **Mappers** (`WebApplication1.Web/Mappers/`) concentram as conversões entre DTOs,
  Commands e modelos de apresentação. Reutilize os profiles do AutoMapper existentes
  e evite espalhar mapeamentos repetidos pelos controllers. Mappers apenas transformam
  dados: não consultam o banco, não persistem alterações e não executam regras de negócio.
- **Injeção de dependência (DI)**: receba as dependências por construtor, preferindo
  contratos de services e repositories. Registre as implementações em
  `WebApplication1.Web/Program.cs`, onde já existem registros `AddScoped` e `AddAutoMapper`.
  Registre também novos profiles de mapeamento. Use lifetime `Scoped` para repositories
  e services que dependam do contexto EF Core; não injete dependências scoped em singletons.
  Não instancie services, repositories ou contextos com `new` no fluxo da aplicação,
  nem use `IServiceProvider` como localizador de serviços para ocultar dependências.
- No fluxo de escrita, o controller recebe o DTO, utiliza o mapper para obter o
  Command e chama o service; o service aplica as regras e utiliza os repositories.
  O controller converte o resultado em uma resposta apropriada ao front-end.
- Views, ViewComponents, TagHelpers e helpers de apresentação não acessam o banco
  diretamente; quando precisarem de dados, utilizam os serviços da aplicação.
- Código legado que mistura essas responsabilidades não serve de padrão para novas
  implementações. Ao modificar um fluxo, organize o código alterado conforme estas
  regras, mantendo a refatoração dentro do escopo da tarefa.

## Convenções principais

- Siga os padrões existentes e mantenha os nomes de domínio em português.
- Não crie ícones em SVG; prefira sempre os ícones gratuitos do Font Awesome já disponíveis no projeto.
- Prefira elementos visuais quadrados, sem `border-radius`, mantendo cantos retos como padrão.
- Formate o código para facilitar a leitura: evite código extremamente compacto, mantenha indentação consistente e use quebras de linha e chaves de forma clara, seguindo os padrões da linguagem em todos os arquivos, incluindo CSS, HTML/Razor, C#, JavaScript e JSON.
- O contexto principal das funcionalidades sociais é `RedditDbContext`,
  com mapeamentos em `Infrastructure/Configurations` e schema padrão do SQL Server (`dbo`), sem configuração explícita.
  Existem contextos legados: confira qual contexto a funcionalidade utiliza.
- Ao alterar entidades persistidas, ajuste o mapeamento e gere a migração correspondente.
  Gerar uma migração não significa que ela foi aplicada ao banco.
- Comentários podem se vincular a um post e a um comentário pai; `IdPost` e
  `IdComentario` são opcionais. O ID do comentário é `int` e o texto é `varchar(3000)`.
- Novos posts, comentários e respostas registram `CriadoEm` em UTC.
  A exibição relativa das datas usa Humanizer e o idioma atual.
- Todas as mensagens e textos da aplicação destinados ao usuário devem vir dos
  resources `resource_ptbr.resx` (português brasileiro) e `resource_en.resx` (inglês).
  Isso inclui títulos, rótulos, botões, placeholders, tooltips, textos de acessibilidade,
  validações, mensagens de sucesso e erro e textos usados pelo JavaScript.
  Não escreva textos de interface diretamente em controllers, services, views ou scripts.
  Toda inclusão ou alteração deve atualizar os dois idiomas, preservando chaves e
  parâmetros equivalentes. Para resultados de negócio, retorne códigos ou resultados
  estruturados e resolva o texto localizado na camada de apresentação.
- Nas views, siga o padrão `IStringLocalizer<SharedResource>` injetado como `Localizer`.
  Preserve as chaves e os parâmetros equivalentes nos dois idiomas.
- Não traduza nomes de rotas, propriedades, identificadores HTML ou conteúdo dos usuários.
- A seleção de idioma usa `culture`, cookie e `Usuario.IdiomaPreferido`, com padrão `EN`.
  Reutilize `_LanguageDropdown` e os layouts existentes.
- A autenticação usa cookies. Preserve a autorização e a proteção antiforgery nas operações de escrita.

## Verificação

Execute os comandos a partir da raiz, indicando o arquivo de projeto explicitamente:

```sh
dotnet build WebApplication1.Web/WebApplication1.Web.csproj
dotnet test WebApplication1.Tests/WebApplication1.Tests.csproj
```

Se a aplicação em execução bloquear os arquivos compilados, use uma pasta de saída
separada dentro de `.build-verification/` com `-p:OutputPath=...`.
Faça verificações proporcionais à alteração e informe limitações relevantes.
