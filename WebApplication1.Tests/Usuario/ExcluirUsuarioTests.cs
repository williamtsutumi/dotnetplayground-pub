using Moq;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services;
using WebApplication1.Application.Services.Usuario;

namespace WebApplication1.Tests.Usuario;

public class ExcluirUsuarioTests
{
    [Fact]
    public void DeletesOnlyRequestedAccount()
    {
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        repository.Setup(r => r.Deletar(42));
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        new UsuarioService(repository.Object, uow.Object, perfilRepository.Object).Deletar(42);
        repository.Verify(r => r.Deletar(42), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsInvalidIdentity(int id)
    {
        var repository = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        Assert.Throws<ArgumentOutOfRangeException>(() => new UsuarioService(repository.Object, uow.Object, perfilRepository.Object).Deletar(id));
        repository.VerifyNoOtherCalls();
    }
}
