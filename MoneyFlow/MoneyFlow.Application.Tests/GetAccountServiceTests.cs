using MoneyFlow.Application.Accounts;
using MoneyFlow.Application.Accounts.GetAccount;
using MoneyFlow.Application.Common.Exceptions;
using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Entities;
using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Application.Tests;

public sealed class GetAccountServiceTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 7, 23, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_ExistingAccount_ReturnsDTO()
    {
        var account = CreateAccount();

        var store = new AccountStoreFake
        {
            AccountToReturn = account
        };

        var service = new GetAccountService(store);

        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;

        var result = await service.ExecuteAsync(account.Id, cancellationToken);

        Assert.Equal(account.Id, result.Id);
        Assert.Equal(account.UserId, result.UserId);
        Assert.Equal("Savings", result.Name);
        Assert.Equal(AccountType.Savings, result.Type);
        Assert.Equal("RUB", result.CurrencyCode);
        Assert.Equal(250.125m, result.OpeningBalance);
        Assert.False(result.IsArchived);
        Assert.Equal(CreatedAt, result.CreatedAt);
        Assert.Null(result.ArchivedAt);

        Assert.Equal(1, store.FindByIdCallCount);
        Assert.Equal(account.Id, store.RequestedAccountId);
        Assert.Equal(cancellationToken, store.ReceivedToken);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyId_ThrowsBeforeStoreAccess()
    {
        var store = new AccountStoreFake();
        var service = new GetAccountService(store);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(Guid.Empty, CancellationToken.None));

        Assert.Equal(0, store.FindByIdCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_MissingAccount_ThrowsNotFoundException()
    {
        var accountId = Guid.NewGuid();
        var store = new AccountStoreFake();
        var service = new GetAccountService(store);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.ExecuteAsync(accountId, CancellationToken.None));

        Assert.Contains(accountId.ToString(), exception.Message);
        Assert.Equal(1, store.FindByIdCallCount);
        Assert.Equal(accountId, store.RequestedAccountId);
    }

    [Fact]
    public async Task ExecuteAsync_CanceledToken_ThrowsBeforeStoreAccess()
    {
        var store = new AccountStoreFake
        {
            AccountToReturn = CreateAccount()
        };

        var service = new GetAccountService(store);

        using var cancellationSource = new CancellationTokenSource();

        cancellationSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.ExecuteAsync(Guid.NewGuid(), cancellationSource.Token));

        Assert.Equal(0, store.FindByIdCallCount);
    }
    
    private static Account CreateAccount()
    {
        return new Account(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Savings",
            AccountType.Savings,
            "RUB",
            Money.Of(250.125m, "RUB"),
            CreatedAt);
    }
    
    private sealed class AccountStoreFake : IAccountStore
    {
        public Account? AccountToReturn { get; init; }

        public int FindByIdCallCount { get; private set; }

        public Guid? RequestedAccountId { get; private set; }

        public CancellationToken? ReceivedToken { get; private set; }

        public Task<Account?> FindByIdAsync(Guid accountId, CancellationToken cancellationToken)
        {
            FindByIdCallCount++;
            RequestedAccountId = accountId;
            ReceivedToken = cancellationToken;

            return Task.FromResult(AccountToReturn);
        }

        public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<bool> NameExistsAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task AddAsync(Account account, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}