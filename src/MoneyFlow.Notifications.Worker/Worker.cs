using System.Text.Json;
using Microsoft.Extensions.Options;
using MoneyFlow.Contracts.Events;
using MoneyFlow.Notifications.Worker.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MoneyFlow.Notifications.Worker;

public class Worker(ILogger<Worker> logger, IOptions<RabbitMqOptions> options) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            ClientProvidedName = "MoneyFlow.Notifications.Worker"
        };
        
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
        logger.LogInformation("Queue created");

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            AccountCreated? message;
            try
            {
                message = JsonSerializer.Deserialize<AccountCreated>(delivery.Body.Span);
            }
            catch (JsonException exception)
            {
                logger.LogWarning(
                    exception,
                    "Invalid AccountCreated JSON. DeliveryTag={DeliveryTag}",
                    delivery.DeliveryTag);

                await channel.BasicRejectAsync(
                    deliveryTag: delivery.DeliveryTag,
                    requeue: false,
                    cancellationToken: cancellationToken);

                return;
            }
            if (message is null ||
                message.EventId == Guid.Empty ||
                message.AccountId == Guid.Empty ||
                message.UserId == Guid.Empty ||
                message.CreatedAt == default)
            {
                logger.LogWarning("Invalid AccountCreated message. DeliveryTag={DeliveryTag}", delivery.DeliveryTag);
                await channel.BasicRejectAsync(deliveryTag: delivery.DeliveryTag, requeue: false, cancellationToken: cancellationToken);
                return;
            }

            logger.LogInformation(
                "Account created notification: EventId={EventId}, " +
                "AccountId={AccountId}, UserId={UserId}, " +
                "CreatedAt={CreatedAt}, Redelivered={Redelivered}", 
                message.EventId, message.AccountId, message.UserId, message.CreatedAt, delivery.Redelivered);
            
            await channel.BasicAckAsync(deliveryTag: delivery.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        };
        
        await channel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
        
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested) { }
    }
}
