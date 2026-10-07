using Aplicacao.Interfaces;
using Entidades.Entidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using WebAPI;
using WebAPI.Token;

namespace xUnitTestAPI;

public class AuthenticationFilterTests : IClassFixture<AuthenticationFilterFactory>
{
    private readonly HttpClient _client;

    public AuthenticationFilterTests(AuthenticationFilterFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/ListarNoticias")]
    [InlineData("/api/ListarNoticiasCustomizadas")]
    [InlineData("/api/AdicionaNoticia")]
    [InlineData("/api/AtualizaNoticia")]
    [InlineData("/api/ExcluirNoticia")]
    [InlineData("/api/BuscarNoticiaPorId")]
    public async Task EndpointProtegido_SemToken_DeveRetornarUnauthorized(string endpoint)
    {
        using var request = CriarPost(endpoint);

        using var resposta = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ComTokenInvalido_DeveRetornarUnauthorized()
    {
        using var request = CriarPost("/api/ListarNoticias");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido");

        using var resposta = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ComTokenValido_DevePermitirAcesso()
    {
        using var request = CriarPost("/api/ListarNoticias");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CriarTokenValido());

        using var resposta = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task EndpointAllowAnonymous_SemToken_DevePermitirCadastro()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/AdicionaUsuarioIdentity")
        {
            Content = new StringContent(
                "{\"email\":\"teste@example.com\",\"senha\":\"Senha123!\",\"idade\":30,\"celular\":\"11999999999\"}",
                Encoding.UTF8,
                "application/json")
        };

        using var resposta = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("Usuário Adicionado com Sucesso", await resposta.Content.ReadAsStringAsync());
    }

    private static HttpRequestMessage CriarPost(string endpoint) => new(HttpMethod.Post, endpoint)
    {
        Content = new StringContent("{}", Encoding.UTF8, "application/json")
    };

    private static string CriarTokenValido() => new TokenJWTBuilder()
        .AddSecurityKey(JwtSecurityKey.Create("Secret_Key-123456789012345678901"))
        .AddSubject("Test user")
        .AddIssuer("Teste.Securiry.Bearer")
        .AddAudience("Teste.Securiry.Bearer")
        .AddClaim("idUsuario", "usuario-teste")
        .AddExpiry(5)
        .Builder()
        .value;
}

public sealed class AuthenticationFilterFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAplicacaoNoticia>();
            var aplicacaoNoticia = new Mock<IAplicacaoNoticia>();
            aplicacaoNoticia.Setup(aplicacao => aplicacao.ListarNoticiasAtivas())
                .ReturnsAsync(new List<Noticia>());
            services.AddSingleton(aplicacaoNoticia.Object);

            var userManager = CriarUserManagerMock();
            userManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), "Senha123!"))
                .ReturnsAsync(IdentityResult.Success);
            userManager.Setup(manager => manager.GetUserIdAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("usuario-teste");
            userManager.Setup(manager => manager.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("token-confirmacao");
            userManager.Setup(manager => manager.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), "token-confirmacao"))
                .ReturnsAsync(IdentityResult.Success);
            services.AddSingleton(userManager.Object);

            var signInManager = new Mock<SignInManager<ApplicationUser>>(
                userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                Options.Create(new IdentityOptions()),
                NullLogger<SignInManager<ApplicationUser>>.Instance,
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<ApplicationUser>>());
            services.AddSingleton(signInManager.Object);
        });
    }

    private static Mock<UserManager<ApplicationUser>> CriarUserManagerMock()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }
}
