namespace MoneyFlow.Domain.Common;

public sealed record Money
{
    public const int CurrencyCodeLength = 3;
    public const int DefaultScale = 4;

    public decimal Amount { get; }

    public string CurrencyCode { get; }

    public bool IsZero => Amount == 0m;

    public bool IsPositive => Amount > 0m;

    public bool IsNegative => Amount < 0m;

    public Money(decimal amount, string currencyCode)
    {
        currencyCode = NormalizeCurrencyCode(currencyCode);

        Amount = decimal.Round(amount, DefaultScale, MidpointRounding.AwayFromZero);
        CurrencyCode = currencyCode;
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, CurrencyCode);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, CurrencyCode);
    }

    public static Money Of(decimal amount, string currencyCode) => new(amount, currencyCode);

    public static Money Zero(string currencyCode) => new(0m, currencyCode);

    private void EnsureSameCurrency(Money? other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (CurrencyCode != other.CurrencyCode)
            throw new InvalidOperationException($"Cannot operate on {CurrencyCode} and {other.CurrencyCode}.");
    }

    private static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = currencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Length != CurrencyCodeLength ||
            normalized.Any(character => character is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency code must contain exactly three Latin letters.",
                nameof(currencyCode));
        }

        return normalized;
    }
}