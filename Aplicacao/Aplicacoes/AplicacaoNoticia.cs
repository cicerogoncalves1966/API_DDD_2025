using Aplicacao.Interfaces;
using Dominio.Interfaces;
using Dominio.Interfaces.InterfacesSericos;
using Entidades.Entidades;

namespace Aplicacao.Aplicacoes
{
    public class AplicacaoNoticia : IAplicacaoNoticia
    {
        INoticia _INoticia;
        IServicoNoticia _IServicoNoticia;

        public AplicacaoNoticia(INoticia iNoticia, IServicoNoticia iServicoNoticia)
        {
            _INoticia = iNoticia;
            _IServicoNoticia = iServicoNoticia;
        }

        public async Task AdicionaNoticia(Noticia noticia)
        {
            await _IServicoNoticia.AdicionaNoticia(noticia);
        }

        public async Task AtualizaNoticia(Noticia noticia)
        {
            await _IServicoNoticia.AtualizaNoticia(noticia);
        }

        public async Task<List<Noticia>> ListarNoticiasAtivas()
        {
            return await _IServicoNoticia.ListarNoticiasAtivas();
        }

        #region Métodos Genéricos
        public async Task Adicionar(Noticia obj)
        {
            await _INoticia.Adicionar(obj);
        }

        public async Task Atualizar(Noticia obj)
        {
            await _INoticia.Atualizar(obj);
        }

        public async Task<Noticia> BuscarPorId(int id)
        {
            return await _INoticia.BuscarPorId(id);
        }

        public async Task Excluir(Noticia obj)
        {
            await _INoticia.Excluir(obj);
        }

        public async Task<List<Noticia>> Listar()
        {
            return await _INoticia.Listar();
        }
        #endregion
    }
}
