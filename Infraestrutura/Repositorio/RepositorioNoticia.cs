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
    }
}
