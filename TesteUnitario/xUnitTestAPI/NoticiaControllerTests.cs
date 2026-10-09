using Aplicacao.Interfaces;
using Entidades.Entidades;
using Entidades.Entidades.ViewModels;
using Entidades.Notificacoes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WebAPI.Controllers;
using WebAPI.Models;

namespace xUnitTestAPI;

public class NoticiaControllerTests
{
    private const string UsuarioId = "usuario-123";

    private readonly Mock<IAplicacaoNoticia> _aplicacao = new();
    private readonly NoticiaController _controller;

    public NoticiaControllerTests()
    {
        _controller = new NoticiaController(_aplicacao.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim("idUsuario", UsuarioId) },
                    "Test"))
            }
        };
    }

    [Fact]
    public async Task ListarNoticias_DeveRetornarNoticiasAtivasDaAplicacao()
    {
        var noticias = new List<Noticia>
        {
            CriarNoticia(1, "Notícia publicada")
        };
        _aplicacao.Setup(a => a.ListarNoticiasAtivas()).ReturnsAsync(noticias);

        var resultado = await _controller.ListarNoticias();

        Assert.Same(noticias, resultado);
        _aplicacao.Verify(a => a.ListarNoticiasAtivas(), Times.Once);
    }

    [Fact]
    public async Task ListarNoticias_QuandoAplicacaoLancaExcecao_DevePropagarExcecao()
    {
        _aplicacao
            .Setup(a => a.ListarNoticiasAtivas())
            .ThrowsAsync(new InvalidOperationException("Falha ao consultar notícias"));

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.ListarNoticias());

        Assert.Equal("Falha ao consultar notícias", excecao.Message);
    }

    [Fact]
    public async Task ListarNoticiasCustomizadas_DeveRetornarResultadoDaAplicacao()
    {
        var noticias = new List<NoticiaViewModel>
        {
            new()
            {
                Id = 4,
                Titulo = "Notícia customizada",
                Informacao = "Conteúdo",
                DataCadastro = "03/02/2025",
                Usuario = "marina"
            }
        };
        _aplicacao.Setup(a => a.ListarNoticiasCustomizadas()).ReturnsAsync(noticias);

        var resultado = await _controller.ListarNoticiasCustomizadas();

        Assert.Same(noticias, resultado);
        _aplicacao.Verify(a => a.ListarNoticiasCustomizadas(), Times.Once);
    }

    [Fact]
    public async Task AdicionaNoticia_DeveEnviarDadosEUsuarioAutenticadoParaAplicacao()
    {
        Noticia? noticiaEnviada = null;
        _aplicacao
            .Setup(a => a.AdicionaNoticia(It.IsAny<Noticia>()))
            .Callback<Noticia>(noticia => noticiaEnviada = noticia)
            .Returns(Task.CompletedTask);

        var resultado = await _controller.AdicionaNoticia(CriarModelo("Novo título", "Novo conteúdo"));

        Assert.Empty(resultado);
        Assert.NotNull(noticiaEnviada);
        Assert.Equal("Novo título", noticiaEnviada.Titulo);
        Assert.Equal("Novo conteúdo", noticiaEnviada.Informacao);
        Assert.Equal(UsuarioId, noticiaEnviada.UserId);
        _aplicacao.Verify(a => a.AdicionaNoticia(It.IsAny<Noticia>()), Times.Once);
    }

    [Fact]
    public async Task AtualizaNoticia_DeveBuscarNoticiaEEnviarAlteracoes()
    {
        var noticiaExistente = CriarNoticia(17, "Título anterior");
        _aplicacao.Setup(a => a.BuscarPorId(17)).ReturnsAsync(noticiaExistente);
        _aplicacao.Setup(a => a.AtualizaNoticia(noticiaExistente)).Returns(Task.CompletedTask);

        var resultado = await _controller.AtualizaNoticia(CriarModelo("Título alterado", "Conteúdo alterado", 17));

        Assert.Empty(resultado);
        Assert.Equal("Título alterado", noticiaExistente.Titulo);
        Assert.Equal("Conteúdo alterado", noticiaExistente.Informacao);
        Assert.Equal(UsuarioId, noticiaExistente.UserId);
        _aplicacao.Verify(a => a.BuscarPorId(17), Times.Once);
        _aplicacao.Verify(a => a.AtualizaNoticia(noticiaExistente), Times.Once);
    }

    [Fact]
    public async Task ExcluirNoticia_DeveBuscarEExcluirNoticia()
    {
        var noticia = CriarNoticia(23, "Notícia para excluir");
        _aplicacao.Setup(a => a.BuscarPorId(23)).ReturnsAsync(noticia);
        _aplicacao.Setup(a => a.Excluir(noticia)).Returns(Task.CompletedTask);

        var resultado = await _controller.ExcluirNoticia(CriarModelo("", "", 23));

        Assert.Same(noticia.Notificacoes, resultado);
        _aplicacao.Verify(a => a.BuscarPorId(23), Times.Once);
        _aplicacao.Verify(a => a.Excluir(noticia), Times.Once);
    }

    [Fact]
    public async Task BuscarNoticiaPorId_DeveRetornarNoticiaEncontrada()
    {
        var noticia = CriarNoticia(31, "Notícia encontrada");
        _aplicacao.Setup(a => a.BuscarPorId(31)).ReturnsAsync(noticia);

        var resultado = await _controller.BuscarNoticiaPorId(CriarModelo("", "", 31));

        Assert.Same(noticia, resultado);
        _aplicacao.Verify(a => a.BuscarPorId(31), Times.Once);
    }

    private static NoticiaModel CriarModelo(string titulo, string informacao, int id = 0) => new()
    {
        IdNoticia = id,
        Titulo = titulo,
        Informacao = informacao,
        IdUsuario = UsuarioId
    };

    private static Noticia CriarNoticia(int id, string titulo) => new()
    {
        Id = id,
        Titulo = titulo,
        Informacao = "Informação de teste"
    };
}
