namespace WebApplication1.Application.Repositories;

public interface IAppGlobalSettingsRepository
{
    Task<string?> Obter(string key, CancellationToken cancellationToken = default);
    void SalvarVersaoDeploy(string version, string deployedAt);
}
