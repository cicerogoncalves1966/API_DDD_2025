using Entidades.Entidades;
using Entidades.Entidades.ViewModels;

namespace Dominio.Interfaces.InterfaceServicos
{
    public interface IServicoNoticia
    {
        Task AdicionaNoticia(Noticia noticia);
        Task AtualizaNoticia(Noticia noticia);
        Task<List<Noticia>> ListarNoticiasAtivas();
        Task<List<NoticiaViewModel>> ListarNoticiasCustomizadas();
    }
}
