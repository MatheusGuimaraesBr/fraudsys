namespace FraudSys.Domain.Entities;

public class LimiteConta
{
    public string Cpf { get; set; }
    public string Agencia { get; set; }
    public string Conta { get; set; }
    public decimal LimitePix { get; set; }

    public LimiteConta(string cpf, string agencia, string conta, decimal limitePix)
    {
        Cpf = cpf;
        Agencia = agencia;
        Conta = conta;
        LimitePix = limitePix;
    }
}