namespace WebApplication1.Application.Records;

public record FeedPaginaRequest(int Pagina, bool TemProxima, List<FeedPostRequest> Posts);
