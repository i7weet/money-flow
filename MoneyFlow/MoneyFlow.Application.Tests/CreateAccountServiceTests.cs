using MoneyFlow.Application.Accounts;
using MoneyFlow.Application.Accounts.CreateAccount;
using MoneyFlow.Application.Common.Exceptions;
using MoneyFlow.Domain.Entities;
using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Application.Tests;

public sealed class CreateAccountServiceTests
{
    private static readonly DateTimeOffset Now = 
        new(2026, 07, 21, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_ValidInput_AddsAccountAndReturnsDTO()
    {
        var store  = new AccountStoreFake()
        {
            UserExists = true,
            NameExists = false,
        };
        
        var service = CreateService(store);
        var input = CreateValidInput();
        
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        
        var result = await service.ExecuteAsync(input, cancellationToken);
        
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(input.UserId, result.UserId);
        Assert.Equal("Main card", result.Name);
        Assert.Equal(AccountType.Card, result.Type);
        Assert.Equal("RUB", result.CurrencyCode);
        Assert.Equal(1000.1235m, result.OpeningBalance);
        Assert.False(result.IsArchived);
        Assert.Equal(Now, result.CreatedAt);
        Assert.Null(result.ArchivedAt);

        Assert.Equal(1, store.UserExistsCallCount);
        Assert.Equal(1, store.NameExistsCallCount);
        Assert.Equal(1, store.AddCallCount);

        Assert.Equal(input.UserId, store.CheckedUserId);
        Assert.Equal("Main card", store.CheckedName);

        Assert.NotNull(store.AddedAccount);
        Assert.Equal(result.Id, store.AddedAccount.Id);
        Assert.Equal(result.Name, store.AddedAccount.Name);
        
        Assert.Equal(3, store.ReceivedTokens.Count);
        Assert.All(store.ReceivedTokens, received => Assert.Equal(cancellationToken, received));
    }
    
    [Fact]
    public async Task ExecuteAsync_UserDoesNotExist_ThrowsNotFoundException()
    {
        var store = new AccountStoreFake
        {
            UserExists = false
        };

        var service = CreateService(store);
        var input = CreateValidInput();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.ExecuteAsync(
                input,
                CancellationToken.None));

        Assert.Contains(input.UserId.ToString(), exception.Message);

        Assert.Equal(1, store.UserExistsCallCount);
        Assert.Equal(0, store.NameExistsCallCount);
        Assert.Equal(0, store.AddCallCount);
        Assert.Null(store.AddedAccount);
    }

    [Fact]
    public async Task ExecuteAsync_NameExists_ThrowsConflictException()
    {
        var store = new AccountStoreFake
        {
            UserExists = true,
            NameExists = true
        };

        var service = CreateService(store);
        var input = CreateValidInput();

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.ExecuteAsync(
                input,
                CancellationToken.None));

        Assert.Contains("Main card", exception.Message);

        Assert.Equal(1, store.UserExistsCallCount);
        Assert.Equal(1, store.NameExistsCallCount);
        Assert.Equal(0, store.AddCallCount);
        Assert.Null(store.AddedAccount);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidInput_ThrowsBeforeStoreAccess()
    {
        var store = new AccountStoreFake
        {
            UserExists = true
        };

        var service = CreateService(store);

        var input = new CreateAccountInputDTO(
            Guid.NewGuid(),
            "   ",
            AccountType.Card,
            "RUB",
            100m);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(
                input,
                CancellationToken.None));

        Assert.Equal(0, store.UserExistsCallCount);
        Assert.Equal(0, store.NameExistsCallCount);
        Assert.Equal(0, store.AddCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_NullInput_ThrowsArgumentNullException()
    {
        var store = new AccountStoreFake();
        var service = CreateService(store);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ExecuteAsync(
                null!,
                CancellationToken.None));

        Assert.Equal(0, store.UserExistsCallCount);
        Assert.Equal(0, store.NameExistsCallCount);
        Assert.Equal(0, store.AddCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_CanceledToken_ThrowsBeforeStoreAccess()
    {
        var store = new AccountStoreFake
        {
            UserExists = true
        };

        var service = CreateService(store);
        var input = CreateValidInput();

        using var cancellationSource =
            new CancellationTokenSource();

        cancellationSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.ExecuteAsync(
                input,
                cancellationSource.Token));

        Assert.Equal(0, store.UserExistsCallCount);
        Assert.Equal(0, store.NameExistsCallCount);
        Assert.Equal(0, store.AddCallCount);
    }
    
    private static CreateAccountService CreateService(AccountStoreFake store) => new (store, new FixedTimeProvider(Now));
    
    private static CreateAccountInputDTO CreateValidInput() =>
        new (Guid.NewGuid(), "  Main card  ", AccountType.Card, "rub", 1000.12345m);

    private sealed class AccountStoreFake : IAccountStore
    {
        public bool UserExists { get; init; } = true;

        public bool NameExists { get; init; }

        public int UserExistsCallCount { get; private set; }

        public int NameExistsCallCount { get; private set; }

        public int AddCallCount { get; private set; }

        public Guid? CheckedUserId { get; private set; }

        public string? CheckedName { get; private set; }

        public Account? AddedAccount { get; private set; }

        public List<CancellationToken> ReceivedTokens { get; } = [];

        public Task<bool> UserExistsAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            UserExistsCallCount++;
            CheckedUserId = userId;
            ReceivedTokens.Add(cancellationToken);

            return Task.FromResult(UserExists);
        }

        public Task<bool> NameExistsAsync(
            Guid userId,
            string name,
            CancellationToken cancellationToken)
        {
            NameExistsCallCount++;
            CheckedName = name;
            ReceivedTokens.Add(cancellationToken);

            return Task.FromResult(NameExists);
        }

        public Task<Account?> FindByIdAsync(
            Guid accountId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Account?>(null);
        }

        public Task AddAsync(
            Account account,
            CancellationToken cancellationToken)
        {
            AddCallCount++;
            AddedAccount = account;
            ReceivedTokens.Add(cancellationToken);

            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}