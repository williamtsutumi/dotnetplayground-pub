using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure;
using WebApplication1.Domain.AppGlobalSettings;

namespace WebApplication1.Tests.Usuario;

public class RedditMappingTests
{
    [Fact]
    public void UserLookupTargetsRedditTable()
    {
        using var context = new RedditDbContext(new DbContextOptionsBuilder<RedditDbContext>()
            .UseNpgsql("Host=localhost;Database=MappingTest;Username=postgres")
            .Options);

        var sql = context.Usuarios.Where(u => u.NomeUsuario == "teste").ToQueryString();

        Assert.Null(context.Model.GetDefaultSchema());
        Assert.All(context.Model.GetEntityTypes(), entity => Assert.Null(entity.GetSchema()));
        Assert.Contains("FROM \"Usuario\"", sql);
        Assert.DoesNotContain("\"Usuarios\"", sql);
        Assert.Contains("\"Senha\"", sql);
        Assert.Contains("\"DataNascimento\"", sql);
    }

    [Fact]
    public void AppGlobalSettingsUsesKeyAsPrimaryKeyAndExpectedTableName()
    {
        using var context = new RedditDbContext(new DbContextOptionsBuilder<RedditDbContext>()
            .UseNpgsql("Host=localhost;Database=MappingTest;Username=postgres")
            .Options);

        var entity = context.Model.FindEntityType(typeof(AppGlobalSetting))!;
        Assert.Equal("AppGlobalSettings", entity.GetTableName());
        Assert.Equal("Key", Assert.Single(entity.FindPrimaryKey()!.Properties).Name);
        Assert.Contains("FROM \"AppGlobalSettings\"", context.AppGlobalSettings.ToQueryString());
    }
}
