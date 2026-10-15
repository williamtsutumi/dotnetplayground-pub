namespace WebApplication1.Domain.Post;

public class Post
{
    public int Id { get; set; }
    public int? ComunidadeId { get; set; }
    public int? IdAutorUsuario { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public bool ConteudoMarkdown { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? EditadoEm { get; set; }

    public Domain.Comunidade.Comunidade? Comunidade { get; set; }

    public bool PodeGravarNovo()
    {
        if (Id != 0)
            return false;

        if (string.IsNullOrEmpty(Titulo) || string.IsNullOrEmpty(Conteudo))
            return false;

        if (IdAutorUsuario == null)
            return false;

        if (EditadoEm != null)
            return false;

        return true;
    }
}
