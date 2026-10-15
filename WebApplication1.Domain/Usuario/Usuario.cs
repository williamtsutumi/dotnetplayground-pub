namespace WebApplication1.Domain.Usuario
{
    public class Usuario
    {
        public object Nome;

        public int Id { get; set; } = 0;
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IdiomaPreferido { get; set; } = "EN";
        public DateTime CriadoEm { get; set; }
        public string? Senha { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Descricao { get; set; }

        public HashSet<Perfil> Perfis { get; set; } = new HashSet<Perfil>();

        public Usuario()
        {
        }

        public bool PodeSerCadastrado()
        {
            return DataNascimento.HasValue && CalculaIdade() <= 150;
        }

        private int CalculaIdade()
        {
            var nascimento = DataNascimento!.Value;
            int yearDiff = DateTime.Now.Year - nascimento.Year;
            if (DateTime.Now.Month < nascimento.Month || (DateTime.Now.Month == nascimento.Month && DateTime.Now.Day < nascimento.Day))
            {
                yearDiff -= 1;
            }
            return yearDiff;
        }
    }
}
