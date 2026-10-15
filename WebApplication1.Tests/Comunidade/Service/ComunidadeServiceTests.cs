using Moq;
using WebApplication1.Application.Records;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services.Comunidade;

namespace WebApplication1.Tests.Comunidade;

public class ComunidadeServiceTests
{
    [Theory]
    [InlineData("Nome de comunidade qualquer", "descrição pequena", 1234)]
    [InlineData("MinhaComunidade!", "", 1234)]
    [InlineData("@*Comunity*@", "salamandra", 1234)]
    [InlineData("@*Comunity*@", "salamandra", 1)]
    [InlineData("@*Comunity*@", "", 1)]
    [InlineData("", "salamandra", 1)]
    public void HappyPathCriarComunidade(string nomeComunidade, string descricao, int idUsuarioCriador)
    {
        var nomeUsuario = "default";

        var comunidadeRepo = new Mock<IComunidadeRepository>();
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var service = new ComunidadeService(comunidadeRepo.Object, usuarioRepo.Object);

        comunidadeRepo.Setup(r => r.IsValidNomeSemDuplicidade(nomeComunidade, 0)).Returns(true);
        usuarioRepo.Setup(r => r.IsValidUsuarioVerificado(nomeUsuario)).Returns(true);

        
        var result = service.Salvar(new CriarComunidadeResponse()
        {
            UsuarioNome = nomeUsuario,
            UsuarioId = idUsuarioCriador,
            Nome = nomeComunidade,
            Descricao = descricao
        });


        Assert.True(result.Sucesso);

        comunidadeRepo.Verify(
            r => r.Salvar(
                It.Is<Domain.Comunidade.Comunidade>(c => c.Nome == nomeComunidade && c.Descricao == descricao && c.IdCriadorUsuario == idUsuarioCriador)
            ),
            Times.Once()
        );
    }

    [Theory]
    [InlineData("MinhaComunidade!", "", 1234)]
    [InlineData("@*Comunity*@", "salamandra", 1234)]
    [InlineData("@*Comunity*@", "salamandra", 1)]
    [InlineData("", "salamandra", 1)]
    public void NaoGravarQuandoNomeComunidadeDuplicado(string nomeComunidade, string descricao, int idUsuarioCriador)
    {
        var nomeUsuario = "default";

        var comunidadeRepo = new Mock<IComunidadeRepository>();
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var service = new ComunidadeService(comunidadeRepo.Object, usuarioRepo.Object);

        comunidadeRepo.Setup(r => r.IsValidNomeSemDuplicidade(nomeComunidade, 0)).Returns(true);
        usuarioRepo.Setup(r => r.IsValidUsuarioVerificado(nomeUsuario)).Returns(false);


        var result = service.Salvar(new CriarComunidadeResponse()
        {
            UsuarioNome = nomeUsuario,
            UsuarioId = idUsuarioCriador,
            Nome = nomeComunidade,
            Descricao = descricao
        });


        Assert.False(result.Sucesso);

        comunidadeRepo.Verify(
            r => r.Salvar(It.IsAny<Domain.Comunidade.Comunidade>()),
            Times.Never()
        );
    }

    [Theory]
    [InlineData("MinhaComunidade!", "", 1234)]
    [InlineData("@*Comunity*@", "salamandra", 1234)]
    [InlineData("@*Comunity*@", "salamandra", 1)]
    [InlineData("", "salamandra", 1)]
    public void NaoGravarQuandoUsuarioNaoVerificado(string nomeComunidade, string descricao, int idUsuarioCriador)
    {
        var nomeUsuario = "default";

        var comunidadeRepo = new Mock<IComunidadeRepository>();
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var service = new ComunidadeService(comunidadeRepo.Object, usuarioRepo.Object);

        comunidadeRepo.Setup(r => r.IsValidNomeSemDuplicidade(nomeComunidade, 0)).Returns(false);
        usuarioRepo.Setup(r => r.IsValidUsuarioVerificado(nomeUsuario)).Returns(true);


        var result = service.Salvar(new CriarComunidadeResponse()
        {
            UsuarioNome = nomeUsuario,
            UsuarioId = idUsuarioCriador,
            Nome = nomeComunidade,
            Descricao = descricao
        });


        Assert.False(result.Sucesso);

        comunidadeRepo.Verify(
            r => r.Salvar(It.IsAny<Domain.Comunidade.Comunidade>()),
            Times.Never()
        );
    }

    [Theory]
    [InlineData(1, "MinhaComunidade!", "QueroMudarONomeDaComunidade", "Descrição cabulosa", 1234)]
    [InlineData(1234, "MinhaComunidade!", "", "Só os de verdade", 14)]
    [InlineData(15, "SuperComunidade", "SuperComunidade", "Descrição", 34)]
    public void DeveSalvarComMesmoNomeQuandoFoiEditado(int id, string nomeComunidadeBanco, string nomeComunidadeDigitado, string descricao, int idUsuarioCriador)
    {
        var nomeUsuario = "default";

        var comunidadeRepo = new Mock<IComunidadeRepository>();
        var usuarioRepo = new Mock<IUsuarioRepository>();
        var service = new ComunidadeService(comunidadeRepo.Object, usuarioRepo.Object);

        comunidadeRepo.Setup(r => r.BuscarPorId(id)).Returns(new Domain.Comunidade.Comunidade()
        {
            Id = id,
            Nome = nomeComunidadeBanco,
            IdCriadorUsuario = idUsuarioCriador
        });
        comunidadeRepo.Setup(r => r.IsValidNomeSemDuplicidade(nomeComunidadeDigitado, id)).Returns(true);
        usuarioRepo.Setup(r => r.IsValidUsuarioVerificado(nomeUsuario)).Returns(true);


        var result = service.Salvar(new CriarComunidadeResponse()
        {
            Id = id,
            UsuarioNome = nomeUsuario,
            UsuarioId = idUsuarioCriador,
            Nome = nomeComunidadeDigitado,
            Descricao = descricao
        });


        Assert.True(result.Sucesso);

        comunidadeRepo.Verify(
            r => r.Salvar(It.Is<Domain.Comunidade.Comunidade>(
                c => c.Nome == nomeComunidadeBanco
                        && c.Descricao == descricao
                        && c.IdCriadorUsuario == idUsuarioCriador
            )),
            Times.Once()
        );
    }

    //[Fact]
    //public void AceitaLimitesDoBanco()
    //{
    //    Assert.True(Validar(new() { Nome = new string('a', 50), Descricao = new string('b', 500) }));
    //    Assert.False(Validar(new() { Nome = new string('a', 51) }));
    //    Assert.False(Validar(new() { Nome = "foto", Descricao = new string('b', 501) }));
    //}


}
