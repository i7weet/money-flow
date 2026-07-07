using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Transaction : Entity
{
    private Transaction()
    {
        Description = string.Empty;
        Amount = new Money(1m, "USD");
    }

    public Transaction(
        Guid id,
        Guid userId,
        Guid primaryAccountId,
        TransactionType type,
        Guid categoryId,
        Money amount,
        string description,
        DateTimeOffset occurredAt,
        Guid? counterAccountId = null,
        Money? counterAmount = null,
        decimal? exchangeRate = null,
        DateTimeOffset? createdAt = null) : base(id)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (primaryAccountId == Guid.Empty)
        {
            throw new ArgumentException("Primary account id cannot be empty.", nameof(primaryAccountId));
        }

        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category id cannot be empty.", nameof(categoryId));
        }

        if (amount.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount must be greater than zero.");
        }

        if (counterAccountId == primaryAccountId)
        {
            throw new ArgumentException("Counter account must differ from primary account.", nameof(counterAccountId));
        }

        if (type == TransactionType.Transfer && counterAccountId is null)
        {
            throw new ArgumentException("Transfer transactions must have a counter account.", nameof(counterAccountId));
        }

        if (type != TransactionType.Transfer && counterAccountId is not null)
        {
            throw new ArgumentException("Only transfer transactions can have a counter account.", nameof(counterAccountId));
        }

        if ((counterAmount is null) != (exchangeRate is null))
        {
            throw new ArgumentException("Counter amount and exchange rate must be provided together.");
        }

        UserId = userId;
        PrimaryAccountId = primaryAccountId;
        CounterAccountId = counterAccountId;
        Type = type;
        CategoryId = categoryId;
        Amount = amount;
        CounterAmount = counterAmount;
        ExchangeRate = exchangeRate;
        Description = NormalizeRequired(description, nameof(description));
        OccurredAt = occurredAt;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }

    public Guid PrimaryAccountId { get; private set; }

    public Guid? CounterAccountId { get; private set; }

    public Guid CategoryId { get; private set; }

    public TransactionType Type { get; private set; }

    public Money Amount { get; private set; }

    public Money? CounterAmount { get; private set; }

    public decimal? ExchangeRate { get; private set; }

    public string Description { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public void UpdateDescription(string description, DateTimeOffset updatedAt)
    {
        Description = NormalizeRequired(description, nameof(description));
        UpdatedAt = updatedAt;
    }

    private static string NormalizeRequired(string? value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        return normalized;
    }
}
