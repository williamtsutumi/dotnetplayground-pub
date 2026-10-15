namespace WebApplication1.Tests.Usuario
{
    public class UsuarioTests
    {
        [Fact]
        public void NaoPermiteUsuarioComMaisDe150Anos()
        {
            var usuario = new Domain.Usuario.Usuario()
            {
                Nome = "Julio da Silva",
                DataNascimento = new DateTime(1850, 01, 01)
            };

            Assert.False(usuario.PodeSerCadastrado());
        }

        [Fact]
        public void PermiteUsuarioComMenosDe150Anos()
        {
            var usuario = new Domain.Usuario.Usuario()
            {
                Nome = "Usuario com 70 anos",
                DataNascimento = new DateTime(1950, 01, 01)
            };

            Assert.True(usuario.PodeSerCadastrado());
        }
    }
}