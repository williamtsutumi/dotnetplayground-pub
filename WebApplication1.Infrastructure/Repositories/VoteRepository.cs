using WebApplication1.Application.Repositories;
using WebApplication1.Domain.UsuarioUpvotePost;

namespace WebApplication1.Infrastructure.Repositories;

public class VoteRepository : IVoteRepository
{
    private readonly RedditDbContext _context;

    public VoteRepository(RedditDbContext context)
    {
        _context = context;
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public UsuarioUpvotePost? BuscarPorPostId(int id)
    {
        return _context.UsuarioUpvotePosts.FirstOrDefault(q => q.IdPost == id);
    }
}
