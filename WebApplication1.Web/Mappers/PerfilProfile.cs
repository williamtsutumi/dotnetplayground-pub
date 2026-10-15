using AutoMapper;
using WebApplication1.Domain.Usuario;
using WebApplication1.Web.Models.Perfil;

namespace WebApplication1.Web.Mappers;

public class PerfilProfile : Profile
{
    public PerfilProfile()
    {
        CreateMap<Usuario, WebApplication1.Models.Perfil.PerfilModel>()
            .ForMember(dest => dest.IdUsuario, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProprioPerfil, opt => opt.Ignore());
        CreateMap<WebApplication1.Web.DTOs.Perfil.EditarDescricaoDto, WebApplication1.Application.Commands.Usuario.EditarDescricaoCommand>();
        CreateMap<Usuario, EditarPerfilModel>().ForMember(
                dest => dest.Nome,
                opt => opt.MapFrom(src => src.NomeUsuario)
            );
    }
}
