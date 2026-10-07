using Aplicacao.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebAPI;
using WebAPI.Token;

namespace xUnitTestAPI;

public class ExceptionHandlingTests : IClassFixture<ExceptionHandlingFactory>
{
    private const string ExceptionMessage = "Sensitive database exception detail";
    private readonly HttpClient _client;

    public ExceptionHandlingTests(ExceptionHandlingFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ApiException_DeveRetornarProblemDetailsSemExporDetalhesInternos()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/ListarNoticias")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CriarTokenValido());

        using var resposta = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();
        using var documento = JsonDocument.Parse(corpo);
        Assert.Equal(500, documento.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain(ExceptionMessage, corpo);
    }

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

public sealed class ExceptionHandlingFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAplicacaoNoticia>();
            var aplicacaoNoticia = new Mock<IAplicacaoNoticia>();
            aplicacaoNoticia
                .Setup(aplicacao => aplicacao.ListarNoticiasAtivas())
                .ThrowsAsync(new InvalidOperationException("Sensitive database exception detail"));
            services.AddSingleton(aplicacaoNoticia.Object);
        });
    }
}
