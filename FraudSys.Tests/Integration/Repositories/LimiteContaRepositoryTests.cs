using Amazon.DynamoDBv2;
using Amazon.Runtime;
using FluentAssertions;
using FraudSys.Domain.Entities;
using FraudSys.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace FraudSys.Tests.Integration.Repositories;

public class LimiteContaRepositoryTests : IAsyncLifetime
{
    private readonly LimiteContaRepository _repository;
    
    private readonly string _cpfTeste = $"999{DateTime.Now.Ticks}";
    private readonly string _contaTeste = $"88{DateTime.Now.Ticks}";

    public LimiteContaRepositoryTests()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var credentials = new BasicAWSCredentials(
            config["AWS:AccessKey"],
            config["AWS:SecretKey"]
        );

        var dynamoConfig = new AmazonDynamoDBConfig
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(config["AWS:Region"])
        };

        var dynamoClient = new AmazonDynamoDBClient(credentials, dynamoConfig);
        _repository = new LimiteContaRepository(dynamoClient);
    }
    
    public async Task InitializeAsync()
    {
        var conta = new LimiteConta(_cpfTeste, "0001", _contaTeste, 1000);
        await _repository.CadastrarAsync(conta);
    }
    
    public async Task DisposeAsync()
    {
        await _repository.RemoverAsync(_cpfTeste, _contaTeste);
    }
    

    [Fact]
    public async Task Cadastrar_DadosValidos_DevePersistitNoDatabase()
    {
        var resultado = await _repository.BuscarAsync(_cpfTeste, _contaTeste);

        resultado.Should().NotBeNull();
        resultado!.Cpf.Should().Be(_cpfTeste);
        resultado.Agencia.Should().Be("0001");
        resultado.Conta.Should().Be(_contaTeste);
        resultado.LimitePix.Should().Be(1000);
    }
    

    [Fact]
    public async Task Buscar_ContaExistente_DeveRetornarDadosCorretos()
    {
        var resultado = await _repository.BuscarAsync(_cpfTeste, _contaTeste);

        resultado.Should().NotBeNull();
        resultado!.LimitePix.Should().Be(1000);
    }

    [Fact]
    public async Task Buscar_ContaInexistente_DeveRetornarNull()
    {
        var resultado = await _repository.BuscarAsync("00000000000", "000000");

        resultado.Should().BeNull();
    }
    

    [Fact]
    public async Task Atualizar_NovoLimite_DeveRefletirNoBanco()
    {
        await _repository.AtualizarLimiteAsync(_cpfTeste, _contaTeste, 5000);

        var resultado = await _repository.BuscarAsync(_cpfTeste, _contaTeste);

        resultado.Should().NotBeNull();
        resultado!.LimitePix.Should().Be(5000);
    }
    

    [Fact]
    public async Task Remover_ContaExistente_DeveNaoExistirMaisNoBanco()
    {
        await _repository.RemoverAsync(_cpfTeste, _contaTeste);

        var resultado = await _repository.BuscarAsync(_cpfTeste, _contaTeste);

        resultado.Should().BeNull();
    }
    

    [Fact]
    public async Task ConsumirLimite_ValorDentroDoLimite_DeveDescontarERetornarTrue()
    {
        var aprovada = await _repository.ConsumirLimiteAsync(_cpfTeste, _contaTeste, 300);

        var resultado = await _repository.BuscarAsync(_cpfTeste, _contaTeste);

        aprovada.Should().BeTrue();
        resultado!.LimitePix.Should().Be(700);
    }

    [Fact]
    public async Task ConsumirLimite_ValorAcimaDoLimite_DeveNaoAlterarLimiteERetornarFalse()
    {
        var aprovada = await _repository.ConsumirLimiteAsync(_cpfTeste, _contaTeste, 9999);

        var resultado = await _repository.BuscarAsync(_cpfTeste, _contaTeste);

        aprovada.Should().BeFalse();
        
        resultado!.LimitePix.Should().Be(1000);
    }
}