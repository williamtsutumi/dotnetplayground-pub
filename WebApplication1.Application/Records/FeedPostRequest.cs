namespace WebApplication1.Application.Records;
public record FeedPostRequest
{
    public int TotalUpvotes { get; set; }
    public int TotalComentarios { get; set; }
    public int Upvotes { get; set; }
    public bool MeuUpvote { get; set; }
    public bool MeuDownvote { get; set; }
    public bool Salvo { get; set; }
    public int Id { get; set; }
    public int? ComunidadeId { get; set; }
    public string Comunidade { get; set; } = string.Empty;
    public int AutorId { get; set; }
    public bool CurrentUserIsAutor { get; set; }
    public string Autor { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public bool ConteudoMarkdown { get; set; }
    public DateTime CriadoEm { get; set; }
}
