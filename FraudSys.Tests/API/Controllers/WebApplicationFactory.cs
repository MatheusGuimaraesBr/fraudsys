using Amazon.DynamoDBv2;
using Amazon.Runtime;
using FraudSys.Application.Services;
using FraudSys.Domain.Interfaces;
using FraudSys.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FraudSys.Tests.API.Controllers;

public class FraudSysWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAmazonDynamoDB));

            if (descriptor != null)
                services.Remove(descriptor);
            
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var credentials = new BasicAWSCredentials(
                config["AWS:AccessKey"],
                config["AWS:SecretKey"]
            );

            var dynamoConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(config["AWS:Region"])
            };

            services.AddSingleton<IAmazonDynamoDB>(
                new AmazonDynamoDBClient(credentials, dynamoConfig));

            services.AddScoped<ILimiteContaRepository, LimiteContaRepository>();
            services.AddScoped<LimiteContaService>();
        });
    }
}