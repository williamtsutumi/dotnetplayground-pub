using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;
using WebApplication1.Models;

namespace WebApplication1.Tests.Usuario;

public class FeedQueryTests
{
    [Fact]
    public void PersonalizedFeedTranslatesMembershipAndGlobalOrderingToSql()
    {
        using var db = new RedditDbContext(new DbContextOptionsBuilder<RedditDbContext>()
            .UseNpgsql("Host=localhost;Database=MappingTest;Username=postgres").Options);
        var sql = FeedQueries.Posts(db, 42).OrderByDescending(p => p.CriadoEm).ThenByDescending(p => p.Id)
            .Skip(20).Take(21).ToQueryString();
        Assert.Contains("\"Post\"", sql);
        Assert.Contains("\"IdCriadorUsuario\"", sql);
        Assert.Contains("EXISTS", sql);
        Assert.Contains("\"ComunidadeUsuario\"", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("\"CriadoEm\" DESC", sql);
        Assert.Contains("OFFSET", sql);
        Assert.DoesNotContain("\"Senha\"", sql);
    }
}
