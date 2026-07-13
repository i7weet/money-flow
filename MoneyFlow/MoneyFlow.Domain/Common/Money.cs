namespace MoneyFlow.Domain.Common;

public sealed record Money
{
    public const int DefaultScale = 4;

    public Money(decimal amount, string currencyCode)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }

        currencyCode = NormalizeCurrencyCode(currencyCode);

        Amount = decimal.Round(amount, DefaultScale, MidpointRounding.AwayFromZero);
        CurrencyCode = currencyCode;
    }

    public decimal Amount { get; }

    public string CurrencyCode { get; }

    public static Money Of(decimal amount, string currencyCode) => new(amount, currencyCode);

    public static Money Zero(string currencyCode) => new(0m, currencyCode);

    private static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = currencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != 3)
        {
            throw new ArgumentException("Currency code must contain exactly three letters.", nameof(currencyCode));
        }

        return normalized;
    }
}

