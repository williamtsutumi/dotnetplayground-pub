using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Comunidade;

public class ComunidadePaginaModel
{
    public Domain.Comunidade.Comunidade Comunidade { get; set; } = new();
    public string Criador { get; set; } = string.Empty;
    public bool Participa { get; set; }
    public bool PodeGerenciar { get; set; }
    public int Membros { get; set; }
    public int Pagina { get; set; }
    public bool TemProxima { get; set; }
    public List<ComunidadePostModel> Posts { get; set; } = new();
}

public class ComunidadePostModel
{
    public int AutorId { get; set; }
    public int TotalComentarios { get; set; }
    public int Id { get; set; }
    public int Upvotes { get; set; }
    public bool MeuUpvote { get; set; }
    public bool MeuDownvote { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public bool ConteudoMarkDown { get; set; }
    public string Autor { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

public class CriarPostModel
{
    public bool ConteudoMarkdown { get; set; }
    public int? PostId { get; set; }

    [StringLength(50, ErrorMessage = "CommunityNameMaximumLengthError")]
    public string? ComunidadeDestino { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public List<string> ComunidadesPermitidas { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public int? ComunidadeId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public string? ComunidadeNome { get; set; }
    [Required(ErrorMessage = "PostTitleRequiredError")]
    [StringLength(300, ErrorMessage = "PostTitleLengthError")]
    public string Titulo { get; set; } = string.Empty;
    [Required(ErrorMessage = "PostContentRequiredError")]
    [StringLength(40000, ErrorMessage = "PostContentLengthError")]
    public string Conteudo { get; set; } = string.Empty;
}
