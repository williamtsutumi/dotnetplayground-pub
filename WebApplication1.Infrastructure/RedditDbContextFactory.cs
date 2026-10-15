using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApplication1.Infrastructure;

public class RedditDbContextFactory : IDesignTimeDbContextFactory<RedditDbContext>
{
    public RedditDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "";

        var options = new DbContextOptionsBuilder<RedditDbContext>()
            .UseNpgsql(connection)
            .Options;
        return new RedditDbContext(options);
    }
}
