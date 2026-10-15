using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.Comunidade;

namespace WebApplication1.Tests.Comunidade;

public class ComunidadeFormTests
{
    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("nome com espaco")]
    [InlineData("fotográfia")]
    [InlineData("<script>")]
    [InlineData("c/teste")]
    public void RejeitaNomesInvalidos(string nome)
    {
        var model = new ComunidadeFormModel { Nome = nome };
        Assert.False(Validar(model));
    }

    [Fact]
    public void AceitaLimitesDoBanco()
    {
        Assert.True(Validar(new() { Nome = new string('a', 50), Descricao = new string('b', 500) }));
        Assert.False(Validar(new() { Nome = new string('a', 51) }));
        Assert.False(Validar(new() { Nome = "foto", Descricao = new string('b', 501) }));
    }

    [Fact]
    public void DescricaoOpcionalENomeComNumerosSaoAceitos()
    {
        Assert.True(Validar(new() { Nome = "Fotos_2026" }));
    }

    private static bool Validar(ComunidadeFormModel model) =>
        Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), true);
}
