using Moq;
using WebApplication1.Application.Commands.Usuario;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services;
using WebApplication1.Application.Services.Usuario;
using Xunit;

namespace WebApplication1.Tests.Usuario;

public class EditarDescricaoTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  Sobre mim  ", "Sobre mim")]
    public void UpdatesOnlyDescriptionForSelectedUser(string? input, string? expected)
    {
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.AtualizarDescricao(42, expected)).Returns(true);
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        var result = new UsuarioService(repository.Object, uow.Object, perfilRepository.Object).AtualizarDescricao(42, new EditarDescricaoCommand { Descricao = input });
        Assert.True(result.Sucesso);
        repository.Verify(r => r.AtualizarDescricao(42, expected), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void RejectsOversizedDescriptionBeforePersistence()
    {
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        var result = new UsuarioService(repository.Object, uow.Object, perfilRepository.Object).AtualizarDescricao(42, new EditarDescricaoCommand { Descricao = new string('a', 1001) });
        Assert.Equal("ProfileDescriptionTooLong", result.CodigoErro);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public void AllowsExactLimitAndReportsMissingUser()
    {
        var text = new string('a', 1000);
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.AtualizarDescricao(42, text)).Returns(false);
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        var result = new UsuarioService(repository.Object, uow.Object, perfilRepository.Object).AtualizarDescricao(42, new EditarDescricaoCommand { Descricao = text });
        Assert.Equal("ProfileNotFound", result.CodigoErro);
        repository.Verify(r => r.AtualizarDescricao(42, text), Times.Once);
    }
}
