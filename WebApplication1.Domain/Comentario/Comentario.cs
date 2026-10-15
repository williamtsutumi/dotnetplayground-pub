namespace WebApplication1.Domain.Comentario;

public class Comentario
{
    public int Id { get; set; }
    public int? IdPost { get; set; }
    public int? IdComentario { get; set; }
    public int? IdAutorUsuario { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
