using AutoMapper;
using WebApplication1.Application.Records;

namespace WebApplication1.Web.Mappers;

public class ComunidadeProfile : Profile
{
    public ComunidadeProfile()
    {
        CreateMap<CriarComunidadeRequest, CriarComunidadeResponse>();
    }
}