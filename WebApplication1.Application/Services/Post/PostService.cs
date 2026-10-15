using WebApplication1.Application.Repositories;

namespace WebApplication1.Application.Services.Post;

public class PostService : IPostService
{
    private readonly IVoteRepository _voteRepository;
    private readonly IPostRepository _postRepository;

    public PostService(IVoteRepository voteRepository, IPostRepository postRepository)
    {
        _voteRepository = voteRepository;
        _postRepository = postRepository;
    }
    public ResultadoService Delete(int id)
    {
        var votes = _voteRepository.BuscarPorPostId(id);
        if (votes != null)
        {
            return ResultadoService.Falha("FalhaNaoPodeExcluirPostComVotos");
        }

        _postRepository.Delete(id);
        _postRepository.SaveChanges();

        return ResultadoService.Ok();
    }
}
