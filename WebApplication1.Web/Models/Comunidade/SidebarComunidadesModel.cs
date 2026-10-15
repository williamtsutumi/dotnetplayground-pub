namespace WebApplication1.Models.Comunidade;

public class SidebarComunidadesModel
{
    public bool Autenticado { get; set; }
    public bool Indisponivel { get; set; }
    public List<SidebarComunidadeItem> Comunidades { get; set; } = new();
}

public class SidebarComunidadeItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool CriadaPorMim { get; set; }
}
