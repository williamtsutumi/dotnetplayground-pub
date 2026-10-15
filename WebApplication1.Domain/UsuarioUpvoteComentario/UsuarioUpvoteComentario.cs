namespace WebApplication1.Domain.UsuarioUpvoteComentario;

public class UsuarioUpvoteComentario
{
    public int IdUsuario { get; set; }
    public int IdComentario { get; set; }
    public TipoVotoComentario Tipo { get; set; } = TipoVotoComentario.UpVote;
}
