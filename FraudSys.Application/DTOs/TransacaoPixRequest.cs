namespace FraudSys.Application.DTOs;

public class TransacaoPixRequest
{
    public string Cpf { get; set; }
    public string Conta { get; set; }
    public decimal Valor { get; set; }
}