using WebApplication1.Domain.Enums;
using WebApplication1.Domain.Feed;

namespace WebApplication1.Tests.Feed;

public class OrdemFeedExtensionsTests
{
    [Theory]
    [InlineData("maisrecentes", OrdemFeed.MaisRecentes)]
    [InlineData("maisantigos", OrdemFeed.MaisAntigos)]
    [InlineData("minhascomunidades", OrdemFeed.MinhasComunidades)]
    [InlineData("invalido", OrdemFeed.MaisRecentes)]
    [InlineData(" ", OrdemFeed.MaisRecentes)]
    [InlineData("represencao que nao existe", OrdemFeed.MaisRecentes)]
    public void StringToEnumDeveRetornarValorEscritoEValorPadrao(string stringOrdemFeed, OrdemFeed esperado)
    {
        Assert.Equal(esperado, stringOrdemFeed.ToEnumOrdemFeed());
    }

    [Theory]
    [InlineData(OrdemFeed.MaisRecentes, "maisrecentes")]
    [InlineData(OrdemFeed.MaisAntigos, "maisantigos")]
    [InlineData(OrdemFeed.MinhasComunidades, "minhascomunidades")]
    public void OrdemFeedToLowerStringRetornaOTextoEsperado(OrdemFeed ordemFeed, string esperado)
    {
        Assert.Equal(esperado, ordemFeed.ToLowerString());
    }

    [Theory]
    [InlineData(OrdemFeed.MaisRecentes, true)]
    [InlineData(OrdemFeed.MaisAntigos, false)]
    [InlineData(OrdemFeed.MinhasComunidades, true)]
    public void IsDescendingDeveRetornarFalseParaMaisAntigas(OrdemFeed ordemFeed, bool esperado)
    {
        Assert.Equal(esperado, ordemFeed.IsDescendingOrder());
    }

    


}
