using WebApplication1.Application.Repositories;

namespace WebApplication1.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly RedditDbContext _context;

    public PostRepository(RedditDbContext context)
    {
        _context = context;
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var post = _context.Posts.FirstOrDefault(q => q.Id == id);
        if (post == null)
            return;

        _context.Posts.Remove(post);
    }
}
