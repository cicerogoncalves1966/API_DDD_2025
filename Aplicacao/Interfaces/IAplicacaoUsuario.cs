namespace Aplicacao.Interfaces
{
    public interface IAplicacaoUsuario
    {
        Task<bool> AdicionaUsuario(string email, string senha, int idade, string celular);
        Task<string> RetornaIdUsuario(string email);
        Task<bool> ExisteUsuario(string email, string senha);
    }
}
