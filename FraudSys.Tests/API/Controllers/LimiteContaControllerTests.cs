using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FraudSys.Application.DTOs;

namespace FraudSys.Tests.API.Controllers;

public class LimiteContaControllerTests : IClassFixture<FraudSysWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly string _cpfTeste = $"777{DateTime.Now.Ticks}";
    private readonly string _contaTeste = $"66{DateTime.Now.Ticks}";

    public LimiteContaControllerTests(FraudSysWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = _cpfTeste,
            Agencia = "0001",
            Conta = _contaTeste,
            LimitePix = 1000
        };

        await _client.PostAsJsonAsync("/api/LimiteConta", request);
    }

    public async Task DisposeAsync()
    {
        await _client.DeleteAsync($"/api/LimiteConta/{_cpfTeste}/{_contaTeste}");
    }
    

    [Fact]
    public async Task Post_DadosValidos_DeveRetornar201()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = $"111{DateTime.Now.Ticks}",
            Agencia = "0001",
            Conta = $"222{DateTime.Now.Ticks}",
            LimitePix = 1000
        };

        var response = await _client.PostAsJsonAsync("/api/LimiteConta", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_CpfVazio_DeveRetornar400()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = "",
            Agencia = "0001",
            Conta = "123456",
            LimitePix = 1000
        };

        var response = await _client.PostAsJsonAsync("/api/LimiteConta", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_LimiteNegativo_DeveRetornar400()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = "12345678900",
            Agencia = "0001",
            Conta = "123456",
            LimitePix = -1
        };

        var response = await _client.PostAsJsonAsync("/api/LimiteConta", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    

    [Fact]
    public async Task Get_ContaExistente_DeveRetornar200()
    {
        var response = await _client.GetAsync($"/api/LimiteConta/{_cpfTeste}/{_contaTeste}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_ContaInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync("/api/LimiteConta/00000000000/000000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    

    [Fact]
    public async Task Put_DadosValidos_DeveRetornar200()
    {
        var request = new AtualizarLimiteRequest { NovoLimite = 2000 };

        var response = await _client.PutAsJsonAsync(
            $"/api/LimiteConta/{_cpfTeste}/{_contaTeste}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_ContaInexistente_DeveRetornar404()
    {
        var request = new AtualizarLimiteRequest { NovoLimite = 2000 };

        var response = await _client.PutAsJsonAsync(
            "/api/LimiteConta/00000000000/000000", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_LimiteNegativo_DeveRetornar400()
    {
        var request = new AtualizarLimiteRequest { NovoLimite = -1 };

        var response = await _client.PutAsJsonAsync(
            $"/api/LimiteConta/{_cpfTeste}/{_contaTeste}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ContaExistente_DeveRetornar200()
    {
        var response = await _client.DeleteAsync(
            $"/api/LimiteConta/{_cpfTeste}/{_contaTeste}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ContaInexistente_DeveRetornar404()
    {
        var response = await _client.DeleteAsync(
            "/api/LimiteConta/00000000000/000000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}