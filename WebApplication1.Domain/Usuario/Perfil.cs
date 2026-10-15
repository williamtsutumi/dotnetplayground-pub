using WebApplication1.Domain.Usuario.Enums;

namespace WebApplication1.Domain.Usuario;

public class Perfil
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public TipoPerfil Tipo { get; set; } = TipoPerfil.Regular;

    public Domain.Usuario.Usuario? Usuario { get; set; }
}
