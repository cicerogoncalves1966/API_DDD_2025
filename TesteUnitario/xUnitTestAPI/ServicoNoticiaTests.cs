using Dominio.Interfaces;
using Dominio.Servicos;
using Entidades.Entidades;
using Moq;
using System.Linq.Expressions;

namespace xUnitTestAPI;

public class ServicoNoticiaTests
{
    [Fact]
    public async Task AdicionaNoticia_ComCamposValidos_DevePersistirComoAtiva()
    {
        var repositorio = new Mock<INoticia>();
        repositorio.Setup(r => r.Adicionar(It.IsAny<Noticia>())).Returns(Task.CompletedTask);
        var servico = new ServicoNoticia(repositorio.Object);
        var noticia = CriarNoticia("Título válido", "Informação válida");

        await servico.AdicionaNoticia(noticia);

        repositorio.Verify(r => r.Adicionar(noticia), Times.Once);
        Assert.True(noticia.Ativo);
        Assert.NotEqual(default, noticia.DataCadastro);
        Assert.NotEqual(default, noticia.DataAlteracao);
    }

    [Theory]
    [InlineData(null, "Informação válida", "Titulo")]
    [InlineData(" ", "Informação válida", "Titulo")]
    [InlineData("Título válido", null, "Informacao")]
    [InlineData("Título válido", "", "Informacao")]
    public async Task AdicionaNoticia_ComCampoObrigatorioVazio_NaoDevePersistir(
        string? titulo,
        string? informacao,
        string propriedadeEsperada)
    {
        var repositorio = new Mock<INoticia>();
        var servico = new ServicoNoticia(repositorio.Object);
        var noticia = CriarNoticia(titulo, informacao);

        await servico.AdicionaNoticia(noticia);

        repositorio.Verify(r => r.Adicionar(It.IsAny<Noticia>()), Times.Never);
        Assert.Contains(noticia.Notificacoes, notificacao => notificacao.NomePropriedade == propriedadeEsperada);
    }

    [Fact]
    public async Task AdicionaNoticia_QuandoRepositorioFalha_DevePropagarExcecao()
    {
        var repositorio = new Mock<INoticia>();
        repositorio
            .Setup(r => r.Adicionar(It.IsAny<Noticia>()))
            .ThrowsAsync(new InvalidOperationException("Falha ao persistir notícia"));
        var servico = new ServicoNoticia(repositorio.Object);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => servico.AdicionaNoticia(CriarNoticia("Título válido", "Informação válida")));

        Assert.Equal("Falha ao persistir notícia", excecao.Message);
    }

    [Fact]
    public async Task AtualizaNoticia_ComCamposValidos_DeveAtualizarComoAtiva()
    {
        var repositorio = new Mock<INoticia>();
        repositorio.Setup(r => r.Atualizar(It.IsAny<Noticia>())).Returns(Task.CompletedTask);
        var servico = new ServicoNoticia(repositorio.Object);
        var noticia = CriarNoticia("Título atualizado", "Informação atualizada");
        noticia.Ativo = false;

        await servico.AtualizaNoticia(noticia);

        repositorio.Verify(r => r.Atualizar(noticia), Times.Once);
        Assert.True(noticia.Ativo);
        Assert.NotEqual(default, noticia.DataAlteracao);
    }

    [Fact]
    public async Task AtualizaNoticia_ComTituloVazio_NaoDevePersistir()
    {
        var repositorio = new Mock<INoticia>();
        var servico = new ServicoNoticia(repositorio.Object);
        var noticia = CriarNoticia(" ", "Informação válida");

        await servico.AtualizaNoticia(noticia);

        repositorio.Verify(r => r.Atualizar(It.IsAny<Noticia>()), Times.Never);
        Assert.Contains(noticia.Notificacoes, notificacao => notificacao.NomePropriedade == "Titulo");
    }

    [Fact]
    public async Task AtualizaNoticia_ComInformacaoVazia_NaoDevePersistir()
    {
        var repositorio = new Mock<INoticia>();
        var servico = new ServicoNoticia(repositorio.Object);
        var noticia = CriarNoticia("Título válido", " ");

        await servico.AtualizaNoticia(noticia);

        repositorio.Verify(r => r.Atualizar(It.IsAny<Noticia>()), Times.Never);
        Assert.Contains(noticia.Notificacoes, notificacao => notificacao.NomePropriedade == "Informacao");
    }

    [Fact]
    public async Task ListarNoticiasAtivas_DeveDelegarConsultaAoRepositorio()
    {
        Expression<Func<Noticia, bool>>? filtroRecebido = null;
        var repositorio = new Mock<INoticia>();
        repositorio
            .Setup(r => r.ListarNoticias(It.IsAny<Expression<Func<Noticia, bool>>>()))
            .Callback<Expression<Func<Noticia, bool>>>(filtro => filtroRecebido = filtro)
            .ReturnsAsync(new List<Noticia>());
        var servico = new ServicoNoticia(repositorio.Object);

        var resultado = await servico.ListarNoticiasAtivas();

        Assert.Empty(resultado);
        Assert.NotNull(filtroRecebido);
        var filtrarAtivas = filtroRecebido.Compile();
        var noticiaAtiva = CriarNoticia("Ativa", "Informação");
        noticiaAtiva.Ativo = true;
        var noticiaInativa = CriarNoticia("Inativa", "Informação");
        noticiaInativa.Ativo = false;
        Assert.True(filtrarAtivas(noticiaAtiva));
        Assert.False(filtrarAtivas(noticiaInativa));
    }

    [Fact]
    public async Task ListarNoticiasCustomizadas_DeveMapearNoticiaEExibirPrefixoDoEmail()
    {
        var noticia = CriarNoticia("Notícia", "Conteúdo");
        noticia.Id = 7;
        noticia.DataCadastro = new DateTime(2025, 2, 3);
        noticia.ApplicationUser = new ApplicationUser { Email = "marina@example.com" };

        var repositorio = new Mock<INoticia>();
        repositorio.Setup(r => r.ListarNoticiasCustomizadas()).ReturnsAsync(new List<Noticia> { noticia });
        var servico = new ServicoNoticia(repositorio.Object);

        var resultado = await servico.ListarNoticiasCustomizadas();

        var noticiaMapeada = Assert.Single(resultado);
        Assert.Equal(noticia.Id, noticiaMapeada.Id);
        Assert.Equal(noticia.Titulo, noticiaMapeada.Titulo);
        Assert.Equal(noticia.Informacao, noticiaMapeada.Informacao);
        Assert.Equal("03/02/2025", noticiaMapeada.DataCadastro);
        Assert.Equal("marina", noticiaMapeada.Usuario);
    }

    private static Noticia CriarNoticia(string? titulo, string? informacao) => new()
    {
        Titulo = titulo!,
        Informacao = informacao!
    };
}
