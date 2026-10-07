using Aplicacao.Interfaces;
using Entidades.Entidades;
using Entidades.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WebAPI.Controllers;
using WebAPI.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace xUnitTestAPI;

public class UsuarioControllerTests
{
    private readonly Mock<IAplicacaoUsuario> _aplicacao = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
    private readonly UsuarioController _controller;

    public UsuarioControllerTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<ApplicationUser>>.Instance);

        _signInManager = new Mock<SignInManager<ApplicationUser>>(
            _userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<ApplicationUser>>.Instance,
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        _controller = new UsuarioController(
            _aplicacao.Object,
            _signInManager.Object,
            _userManager.Object);
    }

    [Theory]
    [InlineData("", "senha-valida")]
    [InlineData("usuario@example.com", " ")]
    public async Task CriarTokenIdentity_ComEmailOuSenhaVazios_DeveRetornarUnauthorized(
        string email,
        string senha)
    {
        var resultado = await _controller.CriarTokenIdentity(CriarLogin(email, senha));

        Assert.IsType<UnauthorizedResult>(resultado);
        _signInManager.Verify(
            manager => manager.PasswordSignInAsync(email, senha, false, false),
            Times.Never);
    }

    [Fact]
    public async Task CriarTokenIdentity_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        _signInManager
            .Setup(manager => manager.PasswordSignInAsync("usuario@example.com", "senha-errada", false, false))
            .ReturnsAsync(SignInResult.Failed);

        var resultado = await _controller.CriarTokenIdentity(CriarLogin("usuario@example.com", "senha-errada"));

        Assert.IsType<UnauthorizedResult>(resultado);
        _aplicacao.Verify(aplicacao => aplicacao.RetornaIdUsuario(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CriarTokenIdentity_QuandoServicoDeLoginLancaExcecao_DevePropagarExcecao()
    {
        _signInManager
            .Setup(manager => manager.PasswordSignInAsync("usuario@example.com", "senha-valida", false, false))
            .ThrowsAsync(new InvalidOperationException("Falha no serviço de autenticação"));

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.CriarTokenIdentity(CriarLogin("usuario@example.com", "senha-valida")));

        Assert.Equal("Falha no serviço de autenticação", excecao.Message);
    }

    [Fact]
    public async Task CriarTokenIdentity_QuandoUsuarioEstaBloqueado_DeveRetornarUnauthorized()
    {
        _signInManager
            .Setup(manager => manager.PasswordSignInAsync("bloqueado@example.com", "senha-valida", false, false))
            .ReturnsAsync(SignInResult.LockedOut);

        var resultado = await _controller.CriarTokenIdentity(CriarLogin("bloqueado@example.com", "senha-valida"));

        Assert.IsType<UnauthorizedResult>(resultado);
        _aplicacao.Verify(aplicacao => aplicacao.RetornaIdUsuario(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CriarTokenIdentity_QuandoUsuarioNaoPossuiId_DeveRetornarNotFound()
    {
        _signInManager
            .Setup(manager => manager.PasswordSignInAsync("usuario@example.com", "senha-valida", false, false))
            .ReturnsAsync(SignInResult.Success);
        _aplicacao.Setup(aplicacao => aplicacao.RetornaIdUsuario("usuario@example.com"))
            .ReturnsAsync((string)null!);

        var resultado = await _controller.CriarTokenIdentity(CriarLogin("usuario@example.com", "senha-valida"));

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task CriarTokenIdentity_ComCredenciaisValidas_DeveRetornarToken()
    {
        _signInManager
            .Setup(manager => manager.PasswordSignInAsync("usuario@example.com", "senha-valida", false, false))
            .ReturnsAsync(SignInResult.Success);
        _aplicacao.Setup(aplicacao => aplicacao.RetornaIdUsuario("usuario@example.com"))
            .ReturnsAsync("usuario-123");

        var resultado = await _controller.CriarTokenIdentity(CriarLogin("usuario@example.com", "senha-valida"));

        var resposta = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<string>(resposta.Value);
        Assert.False(string.IsNullOrWhiteSpace((string?)resposta.Value));
    }

    [Fact]
    public async Task AdicionaUsuarioIdentity_ComEmailOuSenhaVazios_DeveInformarDadosAusentes()
    {
        var resultado = await _controller.AdicionaUsuarioIdentity(CriarUsuario("", "senha"));

        var resposta = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("Falta alguns dados", resposta.Value);
        _userManager.Verify(
            manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task AdicionaUsuarioIdentity_ComDadosValidos_DeveCriarEConfirmarUsuario()
    {
        ApplicationUser? usuarioCriado = null;
        _userManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "senha-segura"))
            .Callback<ApplicationUser, string>((usuario, _) => usuarioCriado = usuario)
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(manager => manager.GetUserIdAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("usuario-456");
        _userManager.Setup(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token-confirmacao");
        _userManager.Setup(manager => manager.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), "token-confirmacao"))
            .ReturnsAsync(IdentityResult.Success);

        var resultado = await _controller.AdicionaUsuarioIdentity(
            CriarUsuario("novo@example.com", "senha-segura", 30, "11999999999"));

        var resposta = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("Usuário Adicionado com Sucesso", resposta.Value);
        Assert.NotNull(usuarioCriado);
        Assert.Equal("novo@example.com", usuarioCriado.Email);
        Assert.Equal("novo@example.com", usuarioCriado.UserName);
        Assert.Equal(30, usuarioCriado.Idade);
        Assert.Equal("11999999999", usuarioCriado.Celular);
        Assert.Equal(TipoUsuario.Operacao, usuarioCriado.Tipo);
        _userManager.Verify(manager => manager.ConfirmEmailAsync(usuarioCriado, "token-confirmacao"), Times.Once);
    }

    [Fact]
    public async Task AdicionaUsuarioIdentity_QuandoCriacaoFalha_DeveRetornarErrosDeIdentity()
    {
        var erros = new[] { new IdentityError { Code = "DuplicateEmail", Description = "E-mail já cadastrado" } };
        _userManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "senha-segura"))
            .ReturnsAsync(IdentityResult.Failed(erros));

        var resultado = await _controller.AdicionaUsuarioIdentity(
            CriarUsuario("existente@example.com", "senha-segura"));

        var resposta = Assert.IsType<OkObjectResult>(resultado);
        var errosRetornados = Assert.IsAssignableFrom<IEnumerable<IdentityError>>(resposta.Value);
        Assert.Contains(errosRetornados, erro => erro.Code == "DuplicateEmail");
        _userManager.Verify(manager => manager.ConfirmEmailAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AdicionaUsuarioIdentity_QuandoConfirmacaoDeEmailFalha_DeveRetornarMensagemDeErro()
    {
        _userManager
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "senha-segura"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(manager => manager.GetUserIdAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("usuario-789");
        _userManager.Setup(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token-invalido");
        _userManager.Setup(manager => manager.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), "token-invalido"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidToken",
                Description = "Token de confirmação inválido"
            }));

        var resultado = await _controller.AdicionaUsuarioIdentity(
            CriarUsuario("novo@example.com", "senha-segura"));

        var resposta = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("Erro ao confirmar usuários", resposta.Value);
    }

    private static LoginModel CriarLogin(string email, string senha) => new()
    {
        email = email,
        senha = senha
    };

    private static UserModel CriarUsuario(string email, string senha, int idade = 0, string celular = "") => new()
    {
        email = email,
        senha = senha,
        idade = idade,
        celular = celular
    };
}
