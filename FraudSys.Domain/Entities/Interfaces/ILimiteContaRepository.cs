using FraudSys.Domain.Entities;

namespace FraudSys.Domain.Interfaces;

public interface ILimiteContaRepository
{
    Task CadastrarAsync(LimiteConta conta);
    Task<LimiteConta?> BuscarAsync(string cpf, string conta);
    Task AtualizarLimiteAsync(string cpf, string conta, decimal novoLimite);
    Task RemoverAsync(string cpf, string conta);
    Task<bool> ConsumirLimiteAsync(string cpf, string conta, decimal valor);
}