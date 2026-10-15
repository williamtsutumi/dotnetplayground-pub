namespace WebApplication1.Domain.Feed;


public static class TipoFeedExtensions
{
    public static string ToLowerString(this TipoFeed ordem)
    {
        return ordem.ToString().ToLower();
    }

    public static TipoFeed ToEnumTipoFeed(this string strEnum)
    {
        if (!Enum.TryParse<TipoFeed>(strEnum, ignoreCase: true, out var ordemEnum))
            return TipoFeed.HomePage;

        return ordemEnum;
    }
}
