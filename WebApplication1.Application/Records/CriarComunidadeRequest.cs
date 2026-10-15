using System.ComponentModel.DataAnnotations;
namespace WebApplication1.Application.Records;

public record CriarComunidadeRequest : IValidatableObject
{
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 3, ErrorMessage = "Use entre 3 e 50 caracteres.")]
    [RegularExpression(@"[a-zA-Z0-9_]+", ErrorMessage = "Use apenas letras sem acentos, números e sublinhado.")]
    [Display(Name = "Nome da comunidade")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Id == 0 && string.IsNullOrWhiteSpace(Nome))
        {
            yield return new ValidationResult(
                "Informe o nome da comunidade.",
                new[] { nameof(Nome) }
            );
        }
    }
}

