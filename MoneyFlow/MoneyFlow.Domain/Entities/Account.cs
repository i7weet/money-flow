using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Account : Entity
{
    private Account()
    {
        Name = string.Empty;
        CurrencyCode = string.Empty;
        OpeningBalance = new Money(1m, "USD");
    }

    public Account(
        Guid id,
        Guid userId,
        string name,
        AccountType type,
        string currencyCode,
        Money openingBalance,
        DateTimeOffset createdAt) : base(id)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        UserId = userId;
        Name = NormalizeRequired(name, nameof(name));
        Type = type;
        CurrencyCode = NormalizeCurrency(currencyCode, nameof(currencyCode));

        if (openingBalance.CurrencyCode != CurrencyCode)
        {
            throw new ArgumentException("Opening balance currency must match account currency.", nameof(openingBalance));
        }

        OpeningBalance = openingBalance;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public AccountType Type { get; private set; }

    public string CurrencyCode { get; private set; }

    public Money OpeningBalance { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    private static string NormalizeRequired(string? value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        return normalized;
    }

    private static string NormalizeCurrency(string? value, string paramName)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != 3)
        {
            throw new ArgumentException("Currency code must contain exactly three letters.", paramName);
        }

        return normalized;
    }
}
