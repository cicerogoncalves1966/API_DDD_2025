using Dominio.Interfaces;
using Dominio.Interfaces.InterfaceServicos;
using Entidades.Entidades;
using Entidades.Entidades.ViewModels;
using System.Globalization;

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
                noticia.DataAlteracao = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToUniversalTime();
                noticia.DataCadastro = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToUniversalTime();
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
                noticia.DataAlteracao = DateTime.Now; // DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToUniversalTime();
                noticia.Ativo = true;

                await _INoticia.Atualizar(noticia);
            }
        }

        public async Task<List<Noticia>> ListarNoticiasAtivas()
        {
            return await _INoticia.ListarNoticias(n => n.Ativo);
        }

        public async Task<List<NoticiaViewModel>> ListarNoticiasCustomizadas()
        {
            var listarNoticiasCustomizadas = await _INoticia.ListarNoticiasCustomizadas();
            var retorno = (
                   from noticia in listarNoticiasCustomizadas
                   select new NoticiaViewModel { 
                       Id = noticia.Id,
                       Titulo = noticia.Titulo,
                       Informacao = noticia.Informacao,
                       DataCadastro = noticia.DataCadastro.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                       Usuario = SeparaEmail(noticia.ApplicationUser.Email)
                   }).ToList();
                
            
            return retorno;
        }

        private string SeparaEmail(string email)
        {
            var emailSeparado = email.Split("@");
            return emailSeparado[0];
        }
    }
}
