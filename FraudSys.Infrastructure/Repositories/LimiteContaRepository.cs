using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using FraudSys.Domain.Entities;
using FraudSys.Domain.Interfaces;

namespace FraudSys.Infrastructure.Repositories;

public class LimiteContaRepository : ILimiteContaRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private const string TableName = "gestor-de-limites";

    public LimiteContaRepository(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;
    }

    public async Task CadastrarAsync(LimiteConta conta)
    {
        var request = new PutItemRequest
        {
            TableName = TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "cpf",       new AttributeValue { S = conta.Cpf } },
                { "conta",     new AttributeValue { S = conta.Conta } },
                { "agencia",   new AttributeValue { S = conta.Agencia } },
                { "limitePix", new AttributeValue { N = conta.LimitePix.ToString() } }
            }
        };

        await _dynamoDb.PutItemAsync(request);
    }

    public async Task<LimiteConta?> BuscarAsync(string cpf, string conta)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "cpf",   new AttributeValue { S = cpf } },
                { "conta", new AttributeValue { S = conta } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        // Se não encontrou, retorna null
        if (!response.Item.Any())
            return null;

        return new LimiteConta(
            cpf:       response.Item["cpf"].S,
            agencia:   response.Item["agencia"].S,
            conta:     response.Item["conta"].S,
            limitePix: decimal.Parse(response.Item["limitePix"].N)
        );
    }

    public async Task AtualizarLimiteAsync(string cpf, string conta, decimal novoLimite)
    {
        var request = new UpdateItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "cpf",   new AttributeValue { S = cpf } },
                { "conta", new AttributeValue { S = conta } }
            },
            UpdateExpression = "SET limitePix = :novoLimite",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":novoLimite", new AttributeValue { N = novoLimite.ToString() } }
            }
        };

        await _dynamoDb.UpdateItemAsync(request);
    }

    public async Task RemoverAsync(string cpf, string conta)
    {
        var request = new DeleteItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "cpf",   new AttributeValue { S = cpf } },
                { "conta", new AttributeValue { S = conta } }
            }
        };

        await _dynamoDb.DeleteItemAsync(request);
    }

    public async Task<bool> ConsumirLimiteAsync(string cpf, string conta, decimal valor)
    {
        var contaAtual = await BuscarAsync(cpf, conta);

        if (contaAtual == null)
            return false;

        if (contaAtual.LimitePix < valor)
            return false;

        var novoLimite = contaAtual.LimitePix - valor;
        await AtualizarLimiteAsync(cpf, conta, novoLimite);

        return true;
    }
}