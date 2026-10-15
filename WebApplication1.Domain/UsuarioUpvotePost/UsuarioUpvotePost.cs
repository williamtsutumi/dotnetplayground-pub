namespace WebApplication1.Domain.UsuarioUpvotePost;

public class UsuarioUpvotePost
{
    public int IdUsuario { get; set; }
    public int IdPost { get; set; }
    public TipoVoto Tipo { get; set; } = TipoVoto.UpVote;
}
