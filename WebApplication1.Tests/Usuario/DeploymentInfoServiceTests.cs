using Moq;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services.AppGlobalSettings;

namespace WebApplication1.Tests.Usuario;

public class DeploymentInfoServiceTests
{
    [Fact]
    public void RegistrarVersaoAtualDelegatesAtomicUpdateToRepository()
    {
        var repository = new Mock<IAppGlobalSettingsRepository>();
        string? writtenVersion = null;
        string? writtenTimestamp = null;
        repository.Setup(x => x.SalvarVersaoDeploy(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((version, timestamp) =>
            {
                writtenVersion = version;
                writtenTimestamp = timestamp;
            });

        new DeploymentInfoService(repository.Object).RegistrarVersaoAtual("1.0.1");

        Assert.Equal("1.0.1", writtenVersion);
        Assert.True(DateTimeOffset.TryParse(writtenTimestamp, out var timestamp));
        Assert.Equal(TimeSpan.Zero, timestamp.Offset);
    }

    [Fact]
    public void RegistrarVersaoAtualWritesTimestampWhenVersionHasNotChanged()
    {
        var repository = new Mock<IAppGlobalSettingsRepository>();
        string? writtenVersion = null;
        string? writtenTimestamp = null;
        repository.Setup(x => x.SalvarVersaoDeploy(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((version, timestamp) =>
            {
                writtenVersion = version;
                writtenTimestamp = timestamp;
            })
            .Verifiable();

        new DeploymentInfoService(repository.Object).RegistrarVersaoAtual("1.0.1");

        repository.Verify(x => x.SalvarVersaoDeploy(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        Assert.Equal("1.0.1", writtenVersion);
        Assert.True(DateTimeOffset.TryParse(writtenTimestamp, out _));
    }

    [Theory]
    [InlineData("2026-01-02T03:04:05+03:00", "2026-01-02T00:04:05+00:00")]
    [InlineData("not-a-date", null)]
    [InlineData(null, null)]
    public async Task ObterUltimoDeployParsesValidUtcAndHandlesMissingOrInvalidValues(string? value, string? expected)
    {
        var repository = new Mock<IAppGlobalSettingsRepository>();
        repository.Setup(x => x.Obter(DeploymentInfoService.LastDeploymentAtKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(value);

        var result = await new DeploymentInfoService(repository.Object).ObterUltimoDeploy();

        Assert.Equal(expected is null ? null : DateTimeOffset.Parse(expected), result);
    }
}
