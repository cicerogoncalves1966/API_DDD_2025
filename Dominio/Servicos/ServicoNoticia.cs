using Dominio.Interfaces.InterfacesSericos;
using Entidades.Entidades;

namespace Dominio.Servicos
{
    public class ServicoNoticia : IServicoNoticia
    {
        public Task AdicionaNoticia(Noticia noticia)
        {
            throw new NotImplementedException();
        }

        public Task AtualizaNoticia(Noticia noticia)
        {
            throw new NotImplementedException();
        }

        public Task<List<Noticia>> ListarNoticiasAtivas()
        {
            throw new NotImplementedException();
        }
    }
}
