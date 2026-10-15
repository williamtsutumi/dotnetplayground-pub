using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;
using WebApplication1.Application.Records;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services;
using WebApplication1.Application.Services.Usuario;

namespace WebApplication1.Tests.Usuario;

public class UsuarioServiceTests
{
    // Assume que o login certo é teste, teste@example.com, Example123!
    [Theory]
    [InlineData(" teste ", "Example123!", true)]
    [InlineData("teste@example.com", "Example123!", true)]
    [InlineData(" teste@example.com ", "Example123!", true)]
    [InlineData("teste@example.com", "wrong-password", false)]
    [InlineData("missing@example.com", "Example123!", false)]
    [InlineData("teste", "wrong-password", false)]
    [InlineData("missing", "Example123!", false)]
    [InlineData("", "Example123!", false)]
    [InlineData("teste", "", false)]
    public async Task AuthenticatesOnlyMatchingCredentials(string name, string password, bool expected)
    {
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        var service = new UsuarioService(usuarioRepo.Object, uow.Object, perfilRepository.Object);
        
        // Senha Example123! hardcoded
        string hash = expected ? "PBKDF2-SHA256$600000$/Xp/9/A2U57xCNJBuYn1jQ==$PfyNYWIFUNKeZchpxwhdYirNxLfbKbUvWCfbvFrPCNs=" : "PBKDF2-SHA256$600000$invalid!$invalid!";

        usuarioRepo
            .Setup(r => r.BuscarPorNomeUsuarioOuEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new Domain.Usuario.Usuario() { Email = name, Nome = name, Senha = hash });


        //service.Salvar(new SalvarUsuarioRequest { Nome = "teste", Email = "teste@example.com", Senha = "Example123!" });
        var result = await service.AutenticarAsync(name, password);


        Assert.Equal(expected, result is not null);
    }

    [Fact]
    public async Task AuthenticatesEmailLongerThanUsernameLimit()
    {
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        var service = new UsuarioService(usuarioRepo.Object, uow.Object, perfilRepository.Object);
        var email = new string('a', 60) + "@example.com";
        service.Salvar(new SalvarUsuarioRequest { Nome = "teste", Email = email, Senha = "Example123!" });
        //Assert.Same(repository.Saved, await service.AutenticarAsync(email, "Example123!"));
        Assert.Null(await service.AutenticarAsync(new string('a', 256), "Example123!"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Example123!")]
    [InlineData("PBKDF2-SHA256$600000$invalid!$invalid!")]
    [InlineData("PBKDF2-SHA256$600000$AA==$AA==")]
    [InlineData("PBKDF2-SHA256$999999999$AA==$AA==")]
    public async Task RejectsMissingOrMalformedHashes(string? hash)
    {
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        var service = new UsuarioService(usuarioRepo.Object, uow.Object, perfilRepository.Object);
        var result = await service.AutenticarAsync("", hash);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "exemplo@exemplo.com", "senha123!")]     // deve ter nome
    [InlineData("  ", "exemplo@user.com", "Example123!")]    // deve ter nome
    [InlineData("teste", "exemplo@exemplo.com", "")]         // deve ter senha
    [InlineData("ADMIN", "ADMIN@exemplo.com", "  ")]         // deve ter senha
    [InlineData("teste", "exemplo", "123")]                  // email invalido
    [InlineData("MeuSuperUser", "invalid.com", "password")]  // email invalido
    public void RejectsInvalidInputBeforeSaving(string name, string email, string password)
    {
        var repository = new Mock<IUsuarioRepository>();
        var perfilRepository = new Mock<IPerfilRepository>();
        var uow = new Mock<IUnitOfWork>();
        var service = new UsuarioService(repository.Object, uow.Object, perfilRepository.Object);

        var result = service.Salvar(new SalvarUsuarioRequest { Nome = name, Email = email, Senha = password });

        Assert.False(result.Sucesso);
    }
}
