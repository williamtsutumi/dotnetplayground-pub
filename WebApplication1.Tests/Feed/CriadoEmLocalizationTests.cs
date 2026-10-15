using System.Globalization;
using Humanizer;

namespace WebApplication1.Tests.Feed;

public class CriadoEmLocalizationTests
{
    [Theory]
    [InlineData("pt-BR", "há 2 horas")]
    [InlineData("en", "2 hours ago")]
    public void Humanize_UsaIdiomaDaInterface(string idioma, string esperado)
    {
        var agora = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        var criadoEm = agora.AddHours(-2);

        var resultado = criadoEm.Humanize(
            utcDate: true,
            dateToCompareAgainst: agora,
            culture: CultureInfo.GetCultureInfo(idioma));

        Assert.Equal(esperado, resultado);
    }
}
