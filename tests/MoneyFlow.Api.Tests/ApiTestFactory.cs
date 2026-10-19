using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoneyFlow.Application.Messaging;
using MoneyFlow.Contracts.Events;

namespace MoneyFlow.Api.Tests;

public sealed class ApiTestFactory: WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMq:HostName"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:UserName"] = "test",
                ["RabbitMq:Password"] = "test",
                ["RabbitMq:QueueName"] = "account-created-test"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAccountCreatedPublisher>();
            services.AddSingleton<IAccountCreatedPublisher>(new AccountCreatedPublisherFake());
        });
    }
    
    private sealed class AccountCreatedPublisherFake : IAccountCreatedPublisher
    {
        public Task PublishAsync(AccountCreated message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    
}