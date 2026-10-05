using Dominio.Interfaces;
using Entidades.Entidades;
using Infraestrutura.Configuracoes;
using Infraestrutura.Repositorio.Genericos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infraestrutura.Repositorio
{
    public class RepositorioNoticia : RepositorioGenerico<Noticia>, INoticia
    {
        private readonly DbContextOptions<Contexto> _optionsBuilder;

        public RepositorioNoticia()
        {
            _optionsBuilder = new DbContextOptions<Contexto>();
        }

        public async Task<List<Noticia>> ListarNoticias(Expression<Func<Noticia, bool>> exNoticia)
        {
            using (var banco = new Contexto(_optionsBuilder))
            {
                return await banco.Noticia.Where(exNoticia).AsNoTracking().ToListAsync();
            }
        }

        public async Task<List<Noticia>> ListarNoticiasCustomizadas()
        {
            using (var banco = new Contexto(_optionsBuilder))
            {
                var listaNoticias = await (from noticia in banco.Noticia
                                     join usuario in banco.ApplicationUser
                                       on noticia.UserId equals usuario.Id
                                     select new Noticia
                                     {
                                         Id = noticia.Id,
                                         Titulo = noticia.Titulo,
                                         Informacao = noticia.Informacao,
                                         DataCadastro = noticia.DataCadastro,
                                         ApplicationUser = usuario
                                     }
                                     ).ToListAsync();
                return listaNoticias;
            }
        }
    }
}
