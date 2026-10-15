using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Web.DTOs.Perfil;

public class EditarDescricaoDto
{
    [StringLength(1000, ErrorMessage = "ProfileDescriptionTooLong")]
    public string? Descricao { get; set; }
}
