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

    if (response.Item == null || !response.Item.Any())
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

    try
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
            ConditionExpression = "limitePix = :limiteAtual",

            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":novoLimite",  new AttributeValue { N = novoLimite.ToString() } },
                { ":limiteAtual", new AttributeValue { N = contaAtual.LimitePix.ToString() } }
            }
        };

        await _dynamoDb.UpdateItemAsync(request);
        return true;
    }
    catch (ConditionalCheckFailedException)
    {
        // Outra requisição alterou o limite antes de nós
        // Retorna false para que a transação seja negada e reprocessada
        return false;
    }
}
}