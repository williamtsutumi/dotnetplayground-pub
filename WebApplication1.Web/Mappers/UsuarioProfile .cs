using AutoMapper;
using WebApplication1.Application.Records;
using WebApplication1.Domain.Usuario;
using WebApplication1.Web.Models.Perfil;

namespace WebApplication1.Web.Mappers;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<SalvarUsuarioRequest, SalvarUsuarioResponse>();
        CreateMap<Usuario, EditarPerfilModel>().ForMember(
                dest => dest.Nome,
                opt => opt.MapFrom(src => src.NomeUsuario)
            );
    }
}