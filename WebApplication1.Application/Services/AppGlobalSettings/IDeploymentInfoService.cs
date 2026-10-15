namespace WebApplication1.Application.Services.AppGlobalSettings;

public interface IDeploymentInfoService
{
    Task<DateTimeOffset?> ObterUltimoDeploy(CancellationToken cancellationToken = default);
    void RegistrarVersaoAtual(string version);
}
