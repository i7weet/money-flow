using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Entities;
using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Domain.Tests.Common;

public sealed class TransactionTests
{
    public static TheoryData<decimal> NonPositiveAmounts => new()
    {
        0m,
        -10m,
        0.00001m
    };

    public static TheoryData<string> InvalidDescriptions => new()
    {
        "",
        "     ",
        new string('A', Transaction.MaxDescriptionLength + 1),
        null!
    };

    public static TheoryData<decimal> NonPositiveCounterAmounts => new()
    {
        0m,
        -10m,
        0.00001m
    };

    public static TheoryData<decimal> NonPositiveExchangeRates => new()
    {
        0m,
        -0.9m
    };

    [Fact]
    public void CreateExpense_WithValidArgs_CreatesExpenseTransaction()
    {
        var data = new TransactionTestData
        {
            Amount = new Money(125.50m, "USD")
        };

        var transaction = CreateExpense(data);

        AssertCommonProperties(transaction, data, TransactionType.Expense);
        Assert.Null(transaction.CounterAccountId);
        Assert.Null(transaction.CounterAmount);
        Assert.Null(transaction.ExchangeRate);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void CreateIncome_WithValidArgs_CreatesIncomeTransaction()
    {
        var data = new TransactionTestData
        {
            Amount = new Money(125.50m, "USD")
        };

        var transaction = CreateIncome(data);

        AssertCommonProperties(transaction, data, TransactionType.Income);
        Assert.Null(transaction.CounterAccountId);
        Assert.Null(transaction.CounterAmount);
        Assert.Null(transaction.ExchangeRate);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void CreateTransfer_WithValidArguments_ReturnsTransferTransaction()
    {
        var data = new TransactionTestData();

        var transaction = CreateTransfer(data);

        AssertCommonProperties(transaction, data, TransactionType.Transfer);
        Assert.Equal(data.CounterAccountId, transaction.CounterAccountId);
        Assert.Equal(data.CounterAmount, transaction.CounterAmount);
        Assert.Equal(data.ExchangeRate, transaction.ExchangeRate);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void CreateExpense_WithEmptyId_ThrowsArgumentException()
    {
        var data = new TransactionTestData { Id = Guid.Empty };

        var exception = Assert.Throws<ArgumentException>(() => CreateExpense(data));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void CreateExpense_WithEmptyUserId_ThrowsArgumentException()
    {
        var data = new TransactionTestData { UserId = Guid.Empty };

        var exception = Assert.Throws<ArgumentException>(() => CreateExpense(data));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void CreateExpense_WithEmptyPrimaryAccountId_ThrowsArgumentException()
    {
        var data = new TransactionTestData { PrimaryAccountId = Guid.Empty };

        var exception = Assert.Throws<ArgumentException>(() => CreateExpense(data));

        Assert.Equal("primaryAccountId", exception.ParamName);
    }

    [Fact]
    public void CreateExpense_WithEmptyCategoryId_ThrowsArgumentException()
    {
        var data = new TransactionTestData { CategoryId = Guid.Empty };

        var exception = Assert.Throws<ArgumentException>(() => CreateExpense(data));

        Assert.Equal("categoryId", exception.ParamName);
    }

    [Fact]
    public void CreateExpense_WithNullAmount_ThrowsArgumentNullException()
    {
        var data = new TransactionTestData { Amount = null };

        var exception = Assert.Throws<ArgumentNullException>(() => CreateExpense(data));

        Assert.Equal("amount", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(NonPositiveAmounts))]
    public void CreateExpense_WithNonPositiveAmount_ThrowsArgumentOutOfRangeException(decimal amountValue)
    {
        var data = new TransactionTestData
        {
            Amount = new Money(amountValue, "USD")
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateExpense(data));

        Assert.Equal("amount", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(InvalidDescriptions))]
    public void CreateExpense_WithInvalidDescription_ThrowsArgumentException(string description)
    {
        var data = new TransactionTestData { Description = description };

        var exception = Assert.Throws<ArgumentException>(() => CreateExpense(data));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void CreateExpense_WithDefaultOccurredAt_ThrowsArgumentException()
    {
        var data = new TransactionTestData { OccurredAt = default };

        var exception = Assert.Throws<ArgumentException>(() => CreateExpense(data));

        Assert.Equal("occurredAt", exception.ParamName);
    }

    [Fact]
    public void CreateExpense_WithDefaultCreatedAt_ThrowsArgumentException()
    {
        var data = new TransactionTestData { CreatedAt = default };

        var exception = Assert.Throws<ArgumentException>(() => CreateExpense(data));

        Assert.Equal("createdAt", exception.ParamName);
    }

    [Fact]
    public void CreateTransfer_WithSameAccounts_ThrowsArgumentException()
    {
        var accountId = Guid.NewGuid();
        var data = new TransactionTestData
        {
            PrimaryAccountId = accountId,
            CounterAccountId = accountId
        };

        var exception = Assert.Throws<ArgumentException>(() => CreateTransfer(data));

        Assert.Equal("counterAccountId", exception.ParamName);
    }

    [Fact]
    public void CreateTransfer_WithEmptyCounterAccountId_ThrowsArgumentException()
    {
        var data = new TransactionTestData { CounterAccountId = Guid.Empty };

        var exception = Assert.Throws<ArgumentException>(() => CreateTransfer(data));

        Assert.Equal("counterAccountId", exception.ParamName);
    }

    [Fact]
    public void CreateTransfer_WithNullCounterAmount_ThrowsArgumentNullException()
    {
        var data = new TransactionTestData { CounterAmount = null };

        var exception = Assert.Throws<ArgumentNullException>(() => CreateTransfer(data));

        Assert.Equal("counterAmount", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(NonPositiveCounterAmounts))]
    public void CreateTransfer_WithNonPositiveCounterAmount_ThrowsArgumentOutOfRangeException(decimal amountValue)
    {
        var data = new TransactionTestData
        {
            CounterAmount = new Money(amountValue, "EUR")
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateTransfer(data));

        Assert.Equal("counterAmount", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(NonPositiveExchangeRates))]
    public void CreateTransfer_WithNonPositiveExchangeRate_ThrowsArgumentOutOfRangeException(decimal exchangeRate)
    {
        var data = new TransactionTestData { ExchangeRate = exchangeRate };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateTransfer(data));

        Assert.Equal("exchangeRate", exception.ParamName);
    }

    [Fact]
    public void CreateTransfer_WithSameCurrencyAndNonUnitRate_ThrowsArgumentException()
    {
        var data = new TransactionTestData
        {
            CounterAmount = new Money(90m, "USD")
        };

        var exception = Assert.Throws<ArgumentException>(() => CreateTransfer(data));

        Assert.Equal("exchangeRate", exception.ParamName);
    }

    [Fact]
    public void CreateTransfer_WithMismatchedCounterAmount_ThrowsArgumentException()
    {
        var data = new TransactionTestData
        {
            CounterAmount = new Money(91m, "EUR")
        };

        var exception = Assert.Throws<ArgumentException>(() => CreateTransfer(data));

        Assert.Equal("counterAmount", exception.ParamName);
    }

    [Fact]
    public void CreateTransfer_WithSameCurrencyAndUnitRate_ReturnsTransferTransaction()
    {
        var data = new TransactionTestData
        {
            CounterAmount = new Money(100m, "USD"),
            ExchangeRate = 1m
        };

        var transaction = CreateTransfer(data);

        Assert.Equal(data.CounterAmount, transaction.CounterAmount);
        Assert.Equal(data.ExchangeRate, transaction.ExchangeRate);
    }

    [Fact]
    public void UpdateDescription_WithValidArguments_UpdatesDescriptionAndTimestamp()
    {
        var transaction = CreateTransfer(new TransactionTestData());
        var updatedAt = transaction.CreatedAt.AddMinutes(10);

        transaction.UpdateDescription(" Updated description ", updatedAt);

        Assert.Equal("Updated description", transaction.Description);
        Assert.Equal(updatedAt, transaction.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(InvalidDescriptions))]
    public void UpdateDescription_WithInvalidDescription_ThrowsArgumentException(string description)
    {
        var transaction = CreateTransfer(new TransactionTestData());
        var updatedAt = transaction.CreatedAt.AddMinutes(10);

        var exception = Assert.Throws<ArgumentException>(() =>
            transaction.UpdateDescription(description, updatedAt));

        Assert.Equal("description", exception.ParamName);
        Assert.Equal("Groceries", transaction.Description);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void UpdateDescription_WithDefaultUpdatedAt_ThrowsArgumentException()
    {
        var transaction = CreateTransfer(new TransactionTestData());

        var exception = Assert.Throws<ArgumentException>(() =>
            transaction.UpdateDescription("Updated description", default));

        Assert.Equal("updatedAt", exception.ParamName);
        Assert.Equal("Groceries", transaction.Description);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void UpdateDescription_WhenUpdatedAtIsBeforeCreatedAt_ThrowsArgumentOutOfRangeException()
    {
        var transaction = CreateTransfer(new TransactionTestData());
        var updatedAt = transaction.CreatedAt.AddTicks(-1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            transaction.UpdateDescription("Updated description", updatedAt));

        Assert.Equal("updatedAt", exception.ParamName);
        Assert.Equal("Groceries", transaction.Description);
        Assert.Null(transaction.UpdatedAt);
    }

    [Fact]
    public void UpdateDescription_WhenUpdatedAtIsBeforePreviousUpdate_ThrowsArgumentOutOfRangeException()
    {
        var transaction = CreateTransfer(new TransactionTestData());
        var firstUpdatedAt = transaction.CreatedAt.AddMinutes(10);
        transaction.UpdateDescription("First update", firstUpdatedAt);
        var secondUpdatedAt = firstUpdatedAt.AddTicks(-1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            transaction.UpdateDescription("Second update", secondUpdatedAt));

        Assert.Equal("updatedAt", exception.ParamName);
        Assert.Equal("First update", transaction.Description);
        Assert.Equal(firstUpdatedAt, transaction.UpdatedAt);
    }

    private static Transaction CreateExpense(TransactionTestData data) =>
        Transaction.CreateExpense(
            id: data.Id,
            userId: data.UserId,
            primaryAccountId: data.PrimaryAccountId,
            categoryId: data.CategoryId,
            amount: data.Amount!,
            description: data.Description!,
            occurredAt: data.OccurredAt,
            createdAt: data.CreatedAt);

    private static Transaction CreateIncome(TransactionTestData data) =>
        Transaction.CreateIncome(
            id: data.Id,
            userId: data.UserId,
            primaryAccountId: data.PrimaryAccountId,
            categoryId: data.CategoryId,
            amount: data.Amount!,
            description: data.Description!,
            occurredAt: data.OccurredAt,
            createdAt: data.CreatedAt);

    private static Transaction CreateTransfer(TransactionTestData data) =>
        Transaction.CreateTransfer(
            id: data.Id,
            userId: data.UserId,
            primaryAccountId: data.PrimaryAccountId,
            categoryId: data.CategoryId,
            amount: data.Amount!,
            description: data.Description!,
            occurredAt: data.OccurredAt,
            createdAt: data.CreatedAt,
            counterAccountId: data.CounterAccountId,
            counterAmount: data.CounterAmount!,
            exchangeRate: data.ExchangeRate);

    private static void AssertCommonProperties(
        Transaction transaction,
        TransactionTestData data,
        TransactionType expectedType)
    {
        Assert.Equal(data.Id, transaction.Id);
        Assert.Equal(data.UserId, transaction.UserId);
        Assert.Equal(data.PrimaryAccountId, transaction.PrimaryAccountId);
        Assert.Equal(data.CategoryId, transaction.CategoryId);
        Assert.Equal(expectedType, transaction.Type);
        Assert.Equal(data.Amount, transaction.Amount);
        Assert.Equal("Groceries", transaction.Description);
        Assert.Equal(data.OccurredAt, transaction.OccurredAt);
        Assert.Equal(data.CreatedAt, transaction.CreatedAt);
    }

    private sealed class TransactionTestData
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public Guid UserId { get; init; } = Guid.NewGuid();

        public Guid PrimaryAccountId { get; init; } = Guid.NewGuid();

        public Guid CategoryId { get; init; } = Guid.NewGuid();

        public Money? Amount { get; init; } = new(100m, "USD");

        public string? Description { get; init; } = " Groceries ";

        public DateTimeOffset OccurredAt { get; init; } =
            new(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);

        public DateTimeOffset CreatedAt { get; init; } =
            new(2026, 1, 10, 12, 1, 0, TimeSpan.Zero);

        public Guid CounterAccountId { get; init; } = Guid.NewGuid();

        public Money? CounterAmount { get; init; } = new(90m, "EUR");

        public decimal ExchangeRate { get; init; } = 0.9m;
    }
}
