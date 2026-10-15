namespace WebApplication1.Models;

public class SearchPageModel
{
    public string Query { get; set; } = string.Empty;
    public string Tipo { get; set; } = "posts";
    public List<FeedPostModel> Posts { get; set; } = new();
    public List<WebApplication1.Domain.Comunidade.Comunidade> Comunidades { get; set; } = new();
    public List<SearchUsuarioModel> Usuarios { get; set; } = new();
    public int TotalPosts { get; set; }
    public int TotalComunidades { get; set; }
    public int TotalUsuarios { get; set; }
    public bool Indisponivel { get; set; }
}

public class SearchUsuarioModel
{
    public int Id { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
}
