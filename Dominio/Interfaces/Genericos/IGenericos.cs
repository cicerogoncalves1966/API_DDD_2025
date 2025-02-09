namespace Dominio.Interfaces.Genericos
{
    public interface IGenericos<T> where T : class
    {
        Task Adicionar(T obj);
        Task Atualizar(T obj);
        Task Excluir(T obj);
        Task<T> BuscarPorId(int id);
        Task<List<T>> Listar();
    }
}
