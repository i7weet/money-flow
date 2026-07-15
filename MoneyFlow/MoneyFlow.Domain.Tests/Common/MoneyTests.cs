using MoneyFlow.Domain.Common;

namespace MoneyFlow.Domain.Tests.Common;

public class MoneyTests
{
    public static TheoryData<string> InvalidCurrencyCodes => new()
    {
        "",
        "   ",
        "US",
        "USDD",
        "U1D",
        "U$D",
        "РУБ"
    };
    
    public static TheoryData<decimal, decimal> RoundingCases => new()
    {
        { 1.23454m, 1.2345m },
        { 1.23455m, 1.2346m },
        { -1.23454m, -1.2345m },
        { -1.23455m, -1.2346m }
    };
    
    public static TheoryData<decimal, bool, bool, bool> SignCases => new()
    {
        // amount, isPositive, isZero, isNegative
        { 10m, true,  false, false },
        { 0m,  false, true,  false },
        { -10m, false, false, true },
        { 0.00001m, false, true, false },
        { -0.00001m, false, true, false }
    };
    
    [Fact]
    public void Constructor_WithWhitespaceAndLowercaseCurrency_NormalizesCurrencyCode()
    {
        var money = new Money(10m, " rub ");
        
        Assert.Equal("RUB", money.CurrencyCode);
    }

    [Theory]
    [MemberData(nameof(RoundingCases))]
    public void Constructor_WithMoreThanFourDecimalPlaces_RoundsAwayFromZero(decimal amount, decimal expected)
    {
        var money = new Money(amount, "USD");
        
        Assert.Equal(expected, money.Amount);
    }

    [Fact]
    public void Zero_WithValidCurrency_ReturnsZeroMoney()
    {
        var money = Money.Zero(" usd ");

        Assert.Equal(0m, money.Amount);
        Assert.Equal("USD", money.CurrencyCode);
        Assert.True(money.IsZero);
        Assert.False(money.IsPositive);
        Assert.False(money.IsNegative);
    }
    
    [Fact]
    public void Constructor_WithNullCurrencyCode_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Money(10m, null!));

        Assert.Equal("currencyCode", exception.ParamName);
    }
    
    [Theory]
    [MemberData(nameof(SignCases))]
    public void Constructor_WithAmount_SetsCorrectSignProperties(
        decimal amount,
        bool expectedIsPositive,
        bool expectedIsZero,
        bool expectedIsNegative)
    {
        var money = new Money(amount, "USD");

        Assert.Equal(expectedIsPositive, money.IsPositive);
        Assert.Equal(expectedIsZero, money.IsZero);
        Assert.Equal(expectedIsNegative, money.IsNegative);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ThrowsInvalidOperationException()
    {
        var moneyRub = new Money(10m, "RUB");
        var moneyEur = new Money(10m, "EUR");
        Assert.Throws<InvalidOperationException>(() => moneyRub.Add(moneyEur));
    }

    [Fact]
    public void Add_WithNull_ThrowsArgumentNullException()
    {
        var  money = new Money(10m, "RUB");
        var exception = Assert.Throws<ArgumentNullException>(
            () => money.Add(null!));

        Assert.Equal("other", exception.ParamName);
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var first = new Money(10m, "RUB");
        var second = new Money(15m, " rub ");
        var result = first.Add(second);
        
        Assert.Equal(25m, result.Amount);
        Assert.Equal("RUB", result.CurrencyCode);
        Assert.Equal(10m, first.Amount);
        Assert.Equal(15m, second.Amount);
        Assert.NotSame(first, result);
        Assert.NotSame(second, result);
    }

    [Fact]
    public void Subtract_WithSameCurrency_ReturnsDifference()
    {
        var first = new  Money(10m, "RUB");
        var second = new  Money(2.5m, "RUB");
        var result = first.Subtract(second);
        
        Assert.Equal(7.5m, result.Amount);
        Assert.Equal("RUB", result.CurrencyCode);
        
        Assert.Equal(10m, first.Amount);
        Assert.Equal(2.5m, second.Amount);
    }

    [Fact]
    public void Subtract_WhenSubtrahendIsGreater_ReturnsNegativeMoney()
    {
        var first = new  Money(5m, "RUB");
        var second = new  Money(8m, "RUB");
        
        var result = first.Subtract(second);
        
        Assert.Equal(-3m, result.Amount);
        Assert.True(result.IsNegative);
    }

    [Fact]
    public void Subtract_WithDifferentCurrencies_ThrowsInvalidOperationException()
    {
        var moneyRub = new Money(10m, "RUB");
        var moneyEur = new Money(10m, "EUR");
        Assert.Throws<InvalidOperationException>(() => moneyRub.Subtract(moneyEur));
    }

    [Fact]
    public void Subtract_WithNull_ThrowsArgumentNullException()
    {
        var moneyRub = new Money(10m, "RUB");
        var exception =  Assert.Throws<ArgumentNullException>(() => moneyRub.Subtract(null!));
        
        Assert.Equal("other", exception.ParamName);
    }

    [Fact]
    public void Of_WithValidValues_ReturnsMoney()
    {
        var money = Money.Of(12.34567m, " eur ");
        
        Assert.Equal(12.3457m, money.Amount);
        Assert.Equal("EUR", money.CurrencyCode);
    }

    [Fact]
    public void Equality_WithEquivalentNormalizedValues_ReturnsTrue()
    {
        var first = Money.Of(12.23455m, " usd ");
        var second = new Money(12.2346m, "USD");
        
        Assert.Equal(first, second);
        Assert.NotSame(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact] public void Equality_WithDifferentAmounts_ReturnsFalse()
    {
        var first = new Money(13.34567m, " eur ");
        var second = new Money(12.345m, "EUR");
        
        Assert.NotEqual(first, second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void Equality_WithDifferentCurrencyCodes_ReturnsFalse()
    {
        var first = new Money(10m, "RUB");
        var second = new Money(10m, "EUR");
        
        Assert.NotEqual(first, second);
        Assert.NotSame(first, second);
    }

    [Theory]
    [MemberData(nameof(InvalidCurrencyCodes))]
    public void Constructor_WithInvalidCurrencyCode_ThrowsArgumentException(string currencyCode)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Money(10m, currencyCode));
        
        Assert.Equal("currencyCode", exception.ParamName);
    }
}