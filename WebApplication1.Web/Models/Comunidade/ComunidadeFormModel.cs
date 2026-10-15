using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Comunidade;

public class ComunidadeFormModel
{
    [Required(ErrorMessage = "CommunityNameRequiredError")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "CommunityNameLengthError")]
    [RegularExpression(@"[a-zA-Z0-9_]+", ErrorMessage = "CommunityNameCharactersError")]
    [Display(Name = "CommunityNameLabel")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "CommunityDescriptionLengthError")]
    [Display(Name = "CommunityDescriptionLabel")]
    public string? Descricao { get; set; }
}
