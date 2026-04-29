using FraudSys.Application.DTOs;
using FraudSys.Domain.Entities;
using FraudSys.Domain.Interfaces;

namespace FraudSys.Application.Services;

public class LimiteContaService
{
    private readonly ILimiteContaRepository _repository;

    public LimiteContaService(ILimiteContaRepository repository)
    {
        _repository = repository;
    }

    public async Task CadastrarAsync(CadastrarLimiteRequest request)
    {
        if (string.IsNullOrEmpty(request.Cpf) ||
            string.IsNullOrEmpty(request.Agencia) ||
            string.IsNullOrEmpty(request.Conta) ||
            request.LimitePix < 0)
            throw new ArgumentException("Todos os campos são obrigatórios e o limite deve ser positivo.");

        var conta = new LimiteConta(
            cpf: request.Cpf,
            agencia: request.Agencia,
            conta: request.Conta,
            limitePix: request.LimitePix
        );

        await _repository.CadastrarAsync(conta);
    }

    public async Task<LimiteConta?> BuscarAsync(string cpf, string conta)
    {
        return await _repository.BuscarAsync(cpf, conta);
    }

    public async Task AtualizarLimiteAsync(string cpf, string conta, AtualizarLimiteRequest request)
    {
        if (request.NovoLimite < 0)
            throw new ArgumentException("O limite não pode ser negativo.");

        var contaExistente = await _repository.BuscarAsync(cpf, conta);
        if (contaExistente == null)
            throw new KeyNotFoundException("Conta não encontrada.");

        await _repository.AtualizarLimiteAsync(cpf, conta, request.NovoLimite);
    }

    public async Task RemoverAsync(string cpf, string conta)
    {
        var contaExistente = await _repository.BuscarAsync(cpf, conta);
        if (contaExistente == null)
            throw new KeyNotFoundException("Conta não encontrada.");

        await _repository.RemoverAsync(cpf, conta);
    }

    public async Task<TransacaoPixResponse> ProcessarTransacaoAsync(TransacaoPixRequest request)
    {
        if (request.Valor <= 0)
            throw new ArgumentException("O valor da transação deve ser maior que zero.");

        var conta = await _repository.BuscarAsync(request.Cpf, request.Conta);
        if (conta == null)
            return new TransacaoPixResponse
            {
                Aprovada = false,
                Mensagem = "Conta não encontrada.",
                LimiteAtual = 0
            };

        var aprovada = await _repository.ConsumirLimiteAsync(
            request.Cpf,
            request.Conta,
            request.Valor
        );

        if (!aprovada)
            return new TransacaoPixResponse
            {
                Aprovada = false,
                Mensagem = "Limite insuficiente para realizar a transação.",
                LimiteAtual = conta.LimitePix
            };

        var contaAtualizada = await _repository.BuscarAsync(request.Cpf, request.Conta);

        return new TransacaoPixResponse
        {
            Aprovada = true,
            Mensagem = "Transação aprovada.",
            LimiteAtual = contaAtualizada!.LimitePix
        };
    }
}