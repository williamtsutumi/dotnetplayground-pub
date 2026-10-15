using WebApplication1.Application.Services;

namespace WebApplication1.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly RedditDbContext _context;

    public UnitOfWork(RedditDbContext context)
    {
        _context = context;
    }

    public T ExecuteInTransaction<T>(Func<T> operation)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var result = operation();
            transaction.Commit();

            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}