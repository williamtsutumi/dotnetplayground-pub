using WebApplication1.Domain.Enums;
using WebApplication1.Domain.Feed;

namespace WebApplication1.Models;


public class FeedModel
{
    private string _ordem = OrdemFeed.MaisRecentes.ToLowerString();
    public string Ordem
    {
        get
        {
            return _ordem;
        }
        set
        {
            _ordem = value.ToEnumOrdemFeed().ToLowerString();
        }
    }

    private string _tipoFeed = Domain.Feed.TipoFeed.PerfilFeedPessoal.ToLowerString();
    public string TipoFeed { get { return _tipoFeed; } set { _tipoFeed = value.ToEnumTipoFeed().ToLowerString(); } }

    public List<FeedPostModel> Posts { get; set; } = new();
    public int Pagina { get; set; } = 1;
    public bool TemProxima { get; set; }
    public bool Indisponivel { get; set; }
}


public class FeedPostModel
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
