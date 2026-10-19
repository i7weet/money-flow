using MoneyFlow.Contracts.Events;

namespace MoneyFlow.Application.Messaging;

public interface IAccountCreatedPublisher
{
    Task PublishAsync(AccountCreated message, CancellationToken cancellationToken);
}