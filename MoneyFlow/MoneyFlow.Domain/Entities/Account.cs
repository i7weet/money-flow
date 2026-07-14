using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Domain.Entities;

public sealed class Account : Entity
{
    public const int MaxNameLength = 128;

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public AccountType Type { get; private set; }

    public string CurrencyCode { get; private set; }

    public Money OpeningBalance { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    private Account()
    {
        Name = string.Empty;
        CurrencyCode = string.Empty;
        OpeningBalance = null!;
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

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown account type.");
        }

        UserId = userId;
        Name = NormalizeName(name, nameof(name));
        Type = type;

        ArgumentNullException.ThrowIfNull(openingBalance);
        CurrencyCode = NormalizeCurrency(currencyCode, nameof(currencyCode));

        if (openingBalance.CurrencyCode != CurrencyCode)
        {
            throw new ArgumentException("Opening balance currency must match account currency.",
                nameof(openingBalance));
        }

        OpeningBalance = openingBalance;
        CreatedAt = ValidateCreatedAt(createdAt, nameof(createdAt));
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Account is already archived.");
        }

        if (archivedAt == default)
        {
            throw new ArgumentException(
                "Archive time must be specified.",
                nameof(archivedAt));
        }

        if (archivedAt < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(archivedAt),
                "Archive time cannot be earlier than creation time.");
        }

        IsArchived = true;
        ArchivedAt = archivedAt;
    }

    private static string NormalizeName(string? value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        if (normalized.Length > MaxNameLength)
        {
            throw new ArgumentException($"Value cannot be longer than {MaxNameLength} characters.", paramName);
        }

        return normalized;
    }

    private static string NormalizeCurrency(string? value, string paramName)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.Length != 3
            || normalized.Any(character => character is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency code must contain exactly three letters.", paramName);
        }

        return normalized;
    }

    private static DateTimeOffset ValidateCreatedAt(DateTimeOffset value, string paramName)
    {
        if (value == default)
        {
            throw new ArgumentException("Creation time must be specified.", paramName);
        }

        return value;
    }

}