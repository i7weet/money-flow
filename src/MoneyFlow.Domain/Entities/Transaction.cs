using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Domain.Entities;

public sealed class Transaction : Entity
{
    public const int MaxDescriptionLength = 512;

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

    private Transaction()
    {
        Description = string.Empty;
        Amount = null!;
    }

    private Transaction(
        Guid id,
        Guid userId,
        Guid primaryAccountId,
        TransactionType type,
        Guid categoryId,
        Money amount,
        string description,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAt,
        Guid? counterAccountId,
        Money? counterAmount,
        decimal? exchangeRate) : base(id)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        if (primaryAccountId == Guid.Empty)
            throw new ArgumentException("Primary account id cannot be empty.", nameof(primaryAccountId));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category id cannot be empty.", nameof(categoryId));

        ArgumentNullException.ThrowIfNull(amount);
        if (amount.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount must be greater than zero.");

        if (counterAccountId == primaryAccountId)
            throw new ArgumentException("Counter account must differ from primary account.", nameof(counterAccountId));

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown transaction type.");

        if (type == TransactionType.Transfer)
        {
            if (counterAmount is null)
                throw new ArgumentNullException(nameof(counterAmount), "Amount must not be null.");
            if (counterAmount.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(counterAmount), "Counter amount must be greater than zero.");
            if (exchangeRate is null || exchangeRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be greater than zero.");
            if (counterAccountId is null || counterAccountId == Guid.Empty)
                throw new ArgumentException("Transfer must have a counter account.", nameof(counterAccountId));
            if (amount.CurrencyCode == counterAmount.CurrencyCode && exchangeRate != 1m)
                throw new ArgumentException("Exchange rate must be 1 for equal currencies.", nameof(exchangeRate));

            var expectedCounterAmount = decimal.Round(
                amount.Amount * exchangeRate.Value,
                Money.DefaultScale,
                MidpointRounding.AwayFromZero);

            if (counterAmount.Amount != expectedCounterAmount)
                throw new ArgumentException("Counter amount does not match amount and exchange rate.", nameof(counterAmount));
        }
        else
        {
            if (counterAccountId is not null)
                throw new ArgumentException("Only transfer transactions can have a counter account.", nameof(counterAccountId));
            if (counterAmount is not null)
                throw new ArgumentException("Only transfer transactions can have a counter amount.", nameof(counterAmount));
            if (exchangeRate is not null)
                throw new ArgumentException("Only transfer transactions can have an exchange rate.", nameof(exchangeRate));
        }

        if (occurredAt == default)
            throw new ArgumentException("Occurrence time must be specified.", nameof(occurredAt));

        if (createdAt == default)
            throw new ArgumentException("Creation time must be specified.", nameof(createdAt));

        UserId = userId;
        PrimaryAccountId = primaryAccountId;
        CounterAccountId = counterAccountId;
        Type = type;
        CategoryId = categoryId;
        Amount = amount;
        CounterAmount = counterAmount;
        ExchangeRate = exchangeRate;
        Description = NormalizeDescription(description, nameof(description));
        OccurredAt = occurredAt;
        CreatedAt = createdAt;
    }

    public static Transaction CreateExpense(
        Guid id,
        Guid userId,
        Guid primaryAccountId,
        Guid categoryId,
        Money amount,
        string description,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAt)
    {
        return new Transaction(
            id,
            userId,
            primaryAccountId,
            TransactionType.Expense,
            categoryId,
            amount,
            description,
            occurredAt,
            createdAt,
            counterAccountId: null,
            counterAmount: null,
            exchangeRate: null);
    }

    public static Transaction CreateIncome(
        Guid id,
        Guid userId,
        Guid primaryAccountId,
        Guid categoryId,
        Money amount,
        string description,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAt)
    {
        return new Transaction(
            id,
            userId,
            primaryAccountId,
            TransactionType.Income,
            categoryId,
            amount,
            description,
            occurredAt,
            createdAt,
            counterAccountId: null,
            counterAmount: null,
            exchangeRate: null);
    }

    public static Transaction CreateTransfer(
        Guid id,
        Guid userId,
        Guid primaryAccountId,
        Guid categoryId,
        Money amount,
        string description,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAt,
        Guid counterAccountId,
        Money counterAmount,
        decimal exchangeRate)
    {
        return new Transaction(
            id,
            userId,
            primaryAccountId,
            TransactionType.Transfer,
            categoryId,
            amount,
            description,
            occurredAt,
            createdAt,
            counterAccountId,
            counterAmount,
            exchangeRate);
    }

    public void UpdateDescription(string description, DateTimeOffset updatedAt)
    {
        var normalizedDescription = NormalizeDescription(description, nameof(description));
        if (updatedAt == default)
            throw new ArgumentException("Update time must be specified.", nameof(updatedAt));

        if (updatedAt < CreatedAt || (UpdatedAt is not null && updatedAt < UpdatedAt.Value))
            throw new ArgumentOutOfRangeException(
                nameof(updatedAt),
                "Update time cannot be earlier than creation time or the previous update.");

        Description = normalizedDescription;
        UpdatedAt = updatedAt;
    }

    private static string NormalizeDescription(string? value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Value cannot be empty.", paramName);
        if (normalized.Length > MaxDescriptionLength)
            throw new ArgumentException($"Value cannot be longer than {MaxDescriptionLength} characters.", paramName);

        return normalized;
    }
}