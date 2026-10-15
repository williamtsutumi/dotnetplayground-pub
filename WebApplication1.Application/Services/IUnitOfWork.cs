namespace WebApplication1.Application.Services;

public interface IUnitOfWork
{
    T ExecuteInTransaction<T>(Func<T> operation);
    void SaveChanges();
}