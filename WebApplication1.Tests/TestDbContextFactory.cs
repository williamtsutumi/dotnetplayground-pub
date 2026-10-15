using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1.Infrastructure;

namespace WebApplication1.Tests;

public static class TestDbContextFactory
{
    public static RedditDbContext CriarRedditDbContext()
    {
        var configurationBuilder = new ConfigurationBuilder();
        var appsettingsWebPath = EncontrarAppsettingsWeb();

        if (appsettingsWebPath is not null)
        {
            configurationBuilder.AddJsonFile(appsettingsWebPath, optional: false);
        }

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure ConnectionStrings:DefaultConnection no appsettings.json de WebApplication1.Web ou pela variável de ambiente DefaultConnection (também aceita ConnectionStrings__DefaultConnection).");
        }

        var options = new DbContextOptionsBuilder<RedditDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var context = new RedditDbContext(options);
        Assert.True(context.Database.CanConnect(), "Não foi possível conectar ao PostgreSQL configurado em ConnectionStrings:DefaultConnection.");

        return context;
    }

    private static string? EncontrarAppsettingsWeb()
    {
        for (var diretorio = new DirectoryInfo(AppContext.BaseDirectory);
             diretorio is not null;
             diretorio = diretorio.Parent)
        {
            var caminho = Path.Combine(diretorio.FullName, "WebApplication1.Web", "appsettings.json");
            if (File.Exists(caminho))
            {
                return caminho;
            }
        }

        return null;
    }
}
