namespace MoneyFlow.Contracts.Events;

public sealed record AccountCreated(Guid EventId, Guid AccountId, Guid UserId, DateTimeOffset CreatedAt);