using Microsoft.AspNetCore.Hosting;
using Moq;

namespace WebApplication1.Tests.Comunidade;

public class ComunidadeViewHtmlTests
{
    [Fact]
    public void HtmlDeveConterPropDeMarkdown()
    {
        var caminho = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "../../../../WebApplication1.Web/Views/Comunidade/View.cshtml"
            )
        );

        var conteudo = File.ReadAllText(caminho);

        Assert.Contains("data-post-markdown", conteudo);
    }
}
