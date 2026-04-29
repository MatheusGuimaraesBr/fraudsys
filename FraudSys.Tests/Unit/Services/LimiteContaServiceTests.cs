using FluentAssertions;
using FraudSys.Application.DTOs;
using FraudSys.Application.Services;
using FraudSys.Domain.Entities;
using FraudSys.Domain.Interfaces;
using Moq;

namespace FraudSys.Tests.Unit.Services;

public class LimiteContaServiceTests
{
    private readonly Mock<ILimiteContaRepository> _repositoryMock;
    private readonly LimiteContaService _service;

    public LimiteContaServiceTests()
    {
        _repositoryMock = new Mock<ILimiteContaRepository>();
        _service = new LimiteContaService(_repositoryMock.Object);
    }
    

    [Fact]
    public async Task Cadastrar_DadosValidos_DeveChamarRepositorio()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = "12345678900",
            Agencia = "0001",
            Conta = "123456",
            LimitePix = 1000
        };
        
        await _service.CadastrarAsync(request);
        
        _repositoryMock.Verify(r => r.CadastrarAsync(It.IsAny<LimiteConta>()), Times.Once);
    }

    [Fact]
    public async Task Cadastrar_CpfVazio_DeveLancarExcecao()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = "",
            Agencia = "0001",
            Conta = "123456",
            LimitePix = 1000
        };

        var act = async () => await _service.CadastrarAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Cadastrar_AgenciaVazia_DeveLancarExcecao()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = "12345678900",
            Agencia = "",
            Conta = "123456",
            LimitePix = 1000
        };

        var act = async () => await _service.CadastrarAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Cadastrar_ContaVazia_DeveLancarExcecao()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = "12345678900",
            Agencia = "0001",
            Conta = "",
            LimitePix = 1000
        };

        var act = async () => await _service.CadastrarAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Cadastrar_LimiteNegativo_DeveLancarExcecao()
    {
        var request = new CadastrarLimiteRequest
        {
            Cpf = "12345678900",
            Agencia = "0001",
            Conta = "123456",
            LimitePix = -1
        };

        var act = async () => await _service.CadastrarAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    

    [Fact]
    public async Task Buscar_ContaExistente_DeveRetornarDados()
    {
        var contaEsperada = new LimiteConta("12345678900", "0001", "123456", 1000);

        _repositoryMock
            .Setup(r => r.BuscarAsync("12345678900", "123456"))
            .ReturnsAsync(contaEsperada);

        var resultado = await _service.BuscarAsync("12345678900", "123456");

        resultado.Should().NotBeNull();
        resultado!.Cpf.Should().Be("12345678900");
        resultado.LimitePix.Should().Be(1000);
    }

    [Fact]
    public async Task Buscar_ContaInexistente_DeveRetornarNull()
    {
        _repositoryMock
            .Setup(r => r.BuscarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((LimiteConta?)null);

        var resultado = await _service.BuscarAsync("00000000000", "000000");

        resultado.Should().BeNull();
    }
    

    [Fact]
    public async Task Atualizar_LimiteValido_DeveAtualizarComSucesso()
    {
        var contaExistente = new LimiteConta("12345678900", "0001", "123456", 1000);

        _repositoryMock
            .Setup(r => r.BuscarAsync("12345678900", "123456"))
            .ReturnsAsync(contaExistente);

        var request = new AtualizarLimiteRequest { NovoLimite = 2000 };

        await _service.AtualizarLimiteAsync("12345678900", "123456", request);

        _repositoryMock.Verify(r => r.AtualizarLimiteAsync("12345678900", "123456", 2000), Times.Once);
    }

    [Fact]
    public async Task Atualizar_LimiteNegativo_DeveLancarExcecao()
    {
        var request = new AtualizarLimiteRequest { NovoLimite = -1 };

        var act = async () => await _service.AtualizarLimiteAsync("12345678900", "123456", request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Atualizar_ContaInexistente_DeveLancarExcecao()
    {
        _repositoryMock
            .Setup(r => r.BuscarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((LimiteConta?)null);

        var request = new AtualizarLimiteRequest { NovoLimite = 2000 };

        var act = async () => await _service.AtualizarLimiteAsync("00000000000", "000000", request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    

    [Fact]
    public async Task Remover_ContaExistente_DeveRemoverComSucesso()
    {
        var contaExistente = new LimiteConta("12345678900", "0001", "123456", 1000);

        _repositoryMock
            .Setup(r => r.BuscarAsync("12345678900", "123456"))
            .ReturnsAsync(contaExistente);

        await _service.RemoverAsync("12345678900", "123456");

        _repositoryMock.Verify(r => r.RemoverAsync("12345678900", "123456"), Times.Once);
    }

    [Fact]
    public async Task Remover_ContaInexistente_DeveLancarExcecao()
    {
        _repositoryMock
            .Setup(r => r.BuscarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((LimiteConta?)null);

        var act = async () => await _service.RemoverAsync("00000000000", "000000");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
    

    [Fact]
    public async Task ProcessarTransacao_DentroDoLimite_DeveAprovarEDescontarValor()
    {
        var conta = new LimiteConta("12345678900", "0001", "123456", 1000);
        var contaAtualizada = new LimiteConta("12345678900", "0001", "123456", 700);

        _repositoryMock
            .Setup(r => r.BuscarAsync("12345678900", "123456"))
            .ReturnsAsync(conta);

        _repositoryMock
            .Setup(r => r.ConsumirLimiteAsync("12345678900", "123456", 300))
            .ReturnsAsync(true);

        _repositoryMock
            .SetupSequence(r => r.BuscarAsync("12345678900", "123456"))
            .ReturnsAsync(conta)
            .ReturnsAsync(contaAtualizada);

        var request = new TransacaoPixRequest
        {
            Cpf = "12345678900",
            Conta = "123456",
            Valor = 300
        };

        var resultado = await _service.ProcessarTransacaoAsync(request);

        resultado.Aprovada.Should().BeTrue();
        resultado.LimiteAtual.Should().Be(700);
    }

    [Fact]
    public async Task ProcessarTransacao_AcimaDoLimite_DeveNegarSemConsumirLimite()
    {
        var conta = new LimiteConta("12345678900", "0001", "123456", 100);

        _repositoryMock
            .Setup(r => r.BuscarAsync("12345678900", "123456"))
            .ReturnsAsync(conta);

        _repositoryMock
            .Setup(r => r.ConsumirLimiteAsync("12345678900", "123456", 500))
            .ReturnsAsync(false);

        var request = new TransacaoPixRequest
        {
            Cpf = "12345678900",
            Conta = "123456",
            Valor = 500
        };

        var resultado = await _service.ProcessarTransacaoAsync(request);

        resultado.Aprovada.Should().BeFalse();
        resultado.LimiteAtual.Should().Be(100);
        
        _repositoryMock.Verify(r => r.AtualizarLimiteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarTransacao_ContaInexistente_DeveNegarTransacao()
    {
        _repositoryMock
            .Setup(r => r.BuscarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((LimiteConta?)null);

        var request = new TransacaoPixRequest
        {
            Cpf = "00000000000",
            Conta = "000000",
            Valor = 100
        };

        var resultado = await _service.ProcessarTransacaoAsync(request);

        resultado.Aprovada.Should().BeFalse();
        resultado.Mensagem.Should().Contain("não encontrada");
    }

    [Fact]
    public async Task ProcessarTransacao_ValorZero_DeveLancarExcecao()
    {
        var request = new TransacaoPixRequest
        {
            Cpf = "12345678900",
            Conta = "123456",
            Valor = 0
        };

        var act = async () => await _service.ProcessarTransacaoAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ProcessarTransacao_ValorNegativo_DeveLancarExcecao()
    {
        var request = new TransacaoPixRequest
        {
            Cpf = "12345678900",
            Conta = "123456",
            Valor = -100
        };

        var act = async () => await _service.ProcessarTransacaoAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}