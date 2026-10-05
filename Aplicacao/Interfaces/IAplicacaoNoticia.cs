using Aplicacao.Interfaces.Genericos;
using Entidades.Entidades;
using Entidades.Entidades.ViewModels;

namespace Aplicacao.Interfaces
{
    public interface IAplicacaoNoticia : IGenericaAplicacao<Noticia>
    {
        Task AdicionaNoticia(Noticia noticia);
        Task AtualizaNoticia(Noticia noticia);
        Task<List<Noticia>> ListarNoticiasAtivas();
        Task<List<NoticiaViewModel>> ListarNoticiasCustomizadas();
    }
}
