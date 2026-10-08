using System.Text.Json;
using Microsoft.Extensions.Options;
using MoneyFlow.Application.Messaging;
using MoneyFlow.Contracts.Events;
using RabbitMQ.Client;

namespace MoneyFlow.Infrastructure.Messaging;

public sealed class RabbitMqAccountCreatedPublisher : IAccountCreatedPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ConnectionFactory _factory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;
    
    public RabbitMqAccountCreatedPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            ClientProvidedName = "MoneyFlow:Api"
        };
    }

    public async Task PublishAsync(AccountCreated message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var channel = await GetChannelAsync(cancellationToken);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true,
                MessageId = message.EventId.ToString()
            };
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                mandatory: true,
                basicProperties: properties,
                body,
                cancellationToken);
        }
        finally
        {
            _gate.Release();
        }

    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_disposed)
                return;
            
            _disposed = true;
            try
            {
                if(_channel is not null)
                    await  _channel.DisposeAsync();
            }
            finally
            {
                if(_connection is not null)
                    await _connection.DisposeAsync();
            }
        }
        finally
        {
            _gate.Release();
        }
    }
    
    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            return _channel;

        _connection ??= await _factory.CreateConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        var channel = await _connection.CreateChannelAsync(channelOptions, cancellationToken);
        try
        {
            await channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
        }
        catch
        {
            await channel.DisposeAsync();
            throw;
        }
        _channel = channel;
        return _channel;
    }
}
