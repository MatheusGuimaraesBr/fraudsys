using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FraudSys.Application.DTOs;

namespace FraudSys.Tests.API.Controllers;

public class TransacaoPixControllerTests : IClassFixture<FraudSysWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly string _cpfTeste = $"555{DateTime.Now.Ticks}";
    private readonly string _contaTeste = $"44{DateTime.Now.Ticks}";

    public TransacaoPixControllerTests(FraudSysWebApplicationFactory factory)
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
    public async Task Post_TransacaoDentroDoLimite_DeveRetornar200()
    {
        var request = new TransacaoPixRequest
        {
            Cpf = _cpfTeste,
            Conta = _contaTeste,
            Valor = 300
        };

        var response = await _client.PostAsJsonAsync("/api/TransacaoPix", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultado = await response.Content.ReadFromJsonAsync<TransacaoPixResponse>();
        resultado!.Aprovada.Should().BeTrue();
        resultado.LimiteAtual.Should().Be(700);
    }

    [Fact]
    public async Task Post_TransacaoAcimaDoLimite_DeveRetornar422()
    {
        var request = new TransacaoPixRequest
        {
            Cpf = _cpfTeste,
            Conta = _contaTeste,
            Valor = 9999
        };

        var response = await _client.PostAsJsonAsync("/api/TransacaoPix", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var resultado = await response.Content.ReadFromJsonAsync<TransacaoPixResponse>();
        resultado!.Aprovada.Should().BeFalse();
        resultado.LimiteAtual.Should().Be(1000);
    }

    [Fact]
    public async Task Post_ContaInexistente_DeveRetornar422()
    {
        var request = new TransacaoPixRequest
        {
            Cpf = "00000000000",
            Conta = "000000",
            Valor = 100
        };

        var response = await _client.PostAsJsonAsync("/api/TransacaoPix", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var resultado = await response.Content.ReadFromJsonAsync<TransacaoPixResponse>();
        resultado!.Aprovada.Should().BeFalse();
    }

    [Fact]
    public async Task Post_ValorZero_DeveRetornar400()
    {
        var request = new TransacaoPixRequest
        {
            Cpf = _cpfTeste,
            Conta = _contaTeste,
            Valor = 0
        };

        var response = await _client.PostAsJsonAsync("/api/TransacaoPix", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}