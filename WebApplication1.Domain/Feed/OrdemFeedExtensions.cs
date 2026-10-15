using WebApplication1.Domain.Enums;

namespace WebApplication1.Domain.Feed;

public static class OrdemFeedExtensions
{
    public static string ToLowerString(this OrdemFeed ordem)
    {
        return ordem.ToString().ToLower();
    }

    public static OrdemFeed ToEnumOrdemFeed(this string strEnum)
    {
        if (!Enum.TryParse<OrdemFeed>(strEnum, ignoreCase: true, out var ordemEnum))
            return OrdemFeed.MaisRecentes;

        if (!Enum.IsDefined(ordemEnum))
            return OrdemFeed.MaisRecentes;

        return ordemEnum;
    }


    public static bool IsDescendingOrder(this OrdemFeed ordem)
    {
        return ordem == OrdemFeed.MaisRecentes || ordem == OrdemFeed.MinhasComunidades || ordem == OrdemFeed.MaisPopulares;
    }

}
