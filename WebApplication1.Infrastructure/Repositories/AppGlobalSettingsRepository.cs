using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Repositories;

namespace WebApplication1.Infrastructure.Repositories;

public sealed class AppGlobalSettingsRepository(RedditDbContext db) : IAppGlobalSettingsRepository
{
    public Task<string?> Obter(string key, CancellationToken cancellationToken = default) =>
        db.AppGlobalSettings.AsNoTracking()
            .Where(setting => setting.Key == key)
            .Select(setting => (string?)setting.Value)
            .SingleOrDefaultAsync(cancellationToken);

    public void SalvarVersaoDeploy(string version, string deployedAt)
    {
        var versionKey = WebApplication1.Application.Services.AppGlobalSettings.DeploymentInfoService.LastDeploymentVersionKey;
        var deployedAtKey = WebApplication1.Application.Services.AppGlobalSettings.DeploymentInfoService.LastDeploymentAtKey;
        // A/B instances can start at the same time. An atomic PostgreSQL upsert
        // avoids primary-key races when the settings rows do not exist yet.
        db.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "AppGlobalSettings" ("Key", "Value")
            VALUES ({versionKey}, {version}), ({deployedAtKey}, {deployedAt})
            ON CONFLICT ("Key") DO UPDATE SET "Value" = EXCLUDED."Value"
            """);
    }
}
