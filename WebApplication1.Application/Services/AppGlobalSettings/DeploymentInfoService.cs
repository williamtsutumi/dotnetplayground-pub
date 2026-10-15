using System.Globalization;
using WebApplication1.Application.Repositories;

namespace WebApplication1.Application.Services.AppGlobalSettings;

public sealed class DeploymentInfoService(IAppGlobalSettingsRepository repository) : IDeploymentInfoService
{
    public const string LastDeploymentVersionKey = "LastDeploymentVersion";
    public const string LastDeploymentAtKey = "LastDeploymentAt";

    public async Task<DateTimeOffset?> ObterUltimoDeploy(CancellationToken cancellationToken = default)
    {
        var value = await repository.Obter(LastDeploymentAtKey, cancellationToken);
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var deployedAt)
            ? deployedAt.ToUniversalTime()
            : null;
    }

    public void RegistrarVersaoAtual(string version)
    {
        var deployedAt = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        repository.SalvarVersaoDeploy(version, deployedAt);
    }
}
