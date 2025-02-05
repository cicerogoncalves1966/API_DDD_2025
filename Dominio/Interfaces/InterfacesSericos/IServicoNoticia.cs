using Entidades.Entidades;

namespace Dominio.Interfaces.InterfacesSericos
{
    public interface IServicoNoticia
    {
        Task AdicionaNoticia(Noticia noticia);
        Task AtualizaNoticia(Noticia noticia);
        Task<List<Noticia>> ListarNoticiasAtivas();
    }
}
