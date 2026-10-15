namespace WebApplication1.Models;

public class PostDetalhesModel
{
    public FeedPostModel Post { get; set; } = new();
    public List<ComentarioModel> Comentarios { get; set; } = new();
    public string NovoComentario { get; set; } = string.Empty;
    public int? RespondendoId { get; set; }
    public string TextoResposta { get; set; } = string.Empty;
    public string ErroResposta { get; set; } = string.Empty;
}

public class ComentarioModel
{
    public int Id { get; set; }
    public int? IdComentario { get; set; }
    public int Profundidade { get; set; }
    public int AutorId { get; set; }
    public string Autor { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
