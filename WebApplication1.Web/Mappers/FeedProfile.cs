using AutoMapper;
using WebApplication1.Application.Records;
using WebApplication1.Models;

namespace WebApplication1.Web.Mappers;

public class FeedProfile : Profile
{
    public FeedProfile()
    {
        CreateMap<FeedPaginaRequest, FeedModel>();
        CreateMap<FeedPostRequest, FeedPostModel>();
    }
}
