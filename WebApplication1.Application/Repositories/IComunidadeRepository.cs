namespace WebApplication1.Application.Repositories;

public interface IComunidadeRepository
{
    public void SaveChanges();
    public void Salvar(Domain.Comunidade.Comunidade Comunidade);
    public bool IsValidNomeSemDuplicidade(string nomeComunidade, int id = 0);
    public Domain.Comunidade.Comunidade? BuscarPorId(int id);
}
