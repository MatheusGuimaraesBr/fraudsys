using Amazon.DynamoDBv2;
using Amazon.Runtime;
using FraudSys.Application.Services;
using FraudSys.Domain.Interfaces;
using FraudSys.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Lê as credenciais diretamente do appsettings.json
// Em Python seria: boto3.client('dynamodb', aws_access_key_id=..., aws_secret_access_key=...)
var awsAccessKey = builder.Configuration["AWS:AccessKey"];
var awsSecretKey = builder.Configuration["AWS:SecretKey"];
var awsRegion = builder.Configuration["AWS:Region"];

var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
var config = new AmazonDynamoDBConfig
{
    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion)
};

builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(credentials, config));

builder.Services.AddScoped<ILimiteContaRepository, LimiteContaRepository>();
builder.Services.AddScoped<LimiteContaService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }