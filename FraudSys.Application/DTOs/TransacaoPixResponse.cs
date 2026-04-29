namespace FraudSys.Application.DTOs;

public class TransacaoPixResponse
{
    public bool Aprovada { get; set; }
    public string Mensagem { get; set; }
    public decimal LimiteAtual { get; set; }
}