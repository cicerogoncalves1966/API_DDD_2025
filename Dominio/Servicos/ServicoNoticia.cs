using Dominio.Interfaces;
using Dominio.Interfaces.InterfacesSericos;
using Entidades.Entidades;

namespace Dominio.Servicos
{
    public class ServicoNoticia : IServicoNoticia
    {
        private readonly INoticia _INoticia;

        public ServicoNoticia(INoticia inoticia)
        {
            _INoticia = inoticia;
        }

        public async Task AdicionaNoticia(Noticia noticia)
        {
            var validarTitulo = noticia.ValidarPropriedadeString(noticia.Titulo, "Titulo");
            var validarInformacao = noticia.ValidarPropriedadeString(noticia.Informacao, "Informacao");

            if (validarTitulo && validarInformacao)
            {
                noticia.DataCadastro = DateTime.Now;
                noticia.Ativo = true;

                await _INoticia.Adicionar(noticia);
            }
        }

        public async Task AtualizaNoticia(Noticia noticia)
        {
            var validarTitulo = noticia.ValidarPropriedadeString(noticia.Titulo, "Titulo");
            var validarInformacao = noticia.ValidarPropriedadeString(noticia.Informacao, "Informacao");

            if (validarTitulo && validarInformacao)
            {
                noticia.DataAlteracao = DateTime.Now;
                noticia.Ativo = true;

                await _INoticia.Atualizar(noticia);
            }
        }

        public async Task<List<Noticia>> ListarNoticiasAtivas()
        {
            return await _INoticia.ListarNoticias(n => n.Ativo);
        }
    }
}
