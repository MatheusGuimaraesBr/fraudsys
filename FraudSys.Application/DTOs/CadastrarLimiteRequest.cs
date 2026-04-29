namespace FraudSys.Application.DTOs;

public class CadastrarLimiteRequest
{
    public string Cpf { get; set; }
    public string Agencia { get; set; }
    public string Conta { get; set; }
    public decimal LimitePix { get; set; }
}