using Aplicacao.Interfaces;
using Entidades.Entidades;
using Entidades.Notificacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticiaController : ControllerBase
    {
        private readonly IAplicacaoNoticia _IAplicaoNoticia;

        public NoticiaController(IAplicacaoNoticia iAplicaoNoticia)
        {
            _IAplicaoNoticia = iAplicaoNoticia;
        }

        [Authorize]
        [Produces("application/json")]
        [HttpPost("/api/ListarNoticias")]
        public async Task<List<Noticia>> ListarNoticias()
        {
            return await _IAplicaoNoticia.ListarNoticiasAtivas();
        }

        [Authorize]
        [Produces("application/json")]
        [HttpPost("/api/AdicionaNoticia")]
        public async Task<List<Notifica>> AdicionaNoticia(NoticiaModel noticia)
        {
            var novaNoticia = new Noticia();
            novaNoticia.Titulo = noticia.Titulo;
            novaNoticia.Informacao = noticia.Informacao;
            novaNoticia.UserId = await RetornaIdUsuarioLogado();

            await _IAplicaoNoticia.AdicionaNoticia(novaNoticia);

            return novaNoticia.Notificacoes;
        }

        [Authorize]
        [Produces("application/json")]
        [HttpPost("/api/AtualizaNoticia")]
        public async Task<List<Notifica>> AtualizaNoticia(NoticiaModel noticia)
        {
            var atualizaNoticia = await _IAplicaoNoticia.BuscarPorId(noticia.IdNoticia);
            atualizaNoticia.Titulo = noticia.Titulo;
            atualizaNoticia.Informacao = noticia.Informacao;
            atualizaNoticia.UserId = await RetornaIdUsuarioLogado();

            await _IAplicaoNoticia.AtualizaNoticia(atualizaNoticia);

            return atualizaNoticia.Notificacoes;
        }

        [Authorize]
        [Produces("application/json")]
        [HttpPost("/api/ExcluirNoticia")]
        public async Task<List<Notifica>> ExcluirNoticia(NoticiaModel noticia)
        {
            var atualizaNoticia = await _IAplicaoNoticia.BuscarPorId(noticia.IdNoticia);

            await _IAplicaoNoticia.Excluir(atualizaNoticia);

            return atualizaNoticia.Notificacoes;
        }

        [Authorize]
        [Produces("application/json")]
        [HttpPost("/api/BuscarNoticiaPorId")]
        public async Task<Noticia> BuscarNoticiaPorId(NoticiaModel noticia)
        {
            var retNoticia = await _IAplicaoNoticia.BuscarPorId(noticia.IdNoticia);

            return retNoticia;
        }

        private async Task<string> RetornaIdUsuarioLogado()
        {
            if (User != null)
            {
                var idUsuario = User.Claims.First().Value;
                return idUsuario;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
