using Microsoft.EntityFrameworkCore;
using MoneyFlow.Application.Common.Exceptions;
using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Entities;
using MoneyFlow.Domain.Enums;
using MoneyFlow.Infrastructure.Persistence;
using Npgsql;

namespace MoneyFlow.Infrastructure.Tests;

public sealed class AccountStoreTests
{
    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task AddAsync_NameTakenAfterCheck_ThrowsConflictException()
    {
        var options = CreateOptions();
        await using var setup = new MoneyFlowDbContext(options);
        await setup.Database.MigrateAsync();
        
        var userId = Guid.NewGuid();
        var accountName = $"Race-{Guid.NewGuid():N}";
        var now = DateTimeOffset.UtcNow;

        try
        {
            setup.Users.Add(new User(userId, $"test-{userId:N}@example.invalid", "test-only-password-hash", now));
            await setup.SaveChangesAsync();

            await using var firstContext = new MoneyFlowDbContext(options);
            await using var secondContext = new MoneyFlowDbContext(options);

            var firstStore = new AccountStore(firstContext);
            var secondStore = new AccountStore(secondContext);

            // Оба запроса видят свободное имя.
            Assert.False(await firstStore.NameExistsAsync(userId, accountName, CancellationToken.None));
            Assert.False(await secondStore.NameExistsAsync(userId, accountName, CancellationToken.None));

            var firstAccount = CreateAccount(userId, accountName, now);
            var secondAccount = CreateAccount(userId, accountName, now);

            await firstStore.AddAsync(firstAccount, CancellationToken.None);

            // Второй запрос уже прошёл проверку,
            // но сохранение теперь нарушает уникальный индекс.
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => secondStore.AddAsync(secondAccount, CancellationToken.None));

            Assert.Contains(accountName, exception.Message);

            await using var verification = new MoneyFlowDbContext(options);

            var savedAccounts = await verification.Accounts
                .AsNoTracking()
                .Where(account => account.UserId == userId)
                .ToListAsync();

            var saved = Assert.Single(savedAccounts);

            Assert.Equal(firstAccount.Id, saved.Id);
            Assert.Equal(accountName, saved.Name);
        }
        finally
        {
            // Удаляем только данные текущего теста.
            // После ошибки сохранения используем новый DbContext.
            await using var cleanup = new MoneyFlowDbContext(options);
            await cleanup.Accounts.Where(account => account.UserId == userId).ExecuteDeleteAsync();
            await cleanup.Users.Where(user => user.Id == userId).ExecuteDeleteAsync();
        }
    }

    private static Account CreateAccount(Guid userId, string name, DateTimeOffset createdAt)
    {
        return new Account(
            Guid.NewGuid(),
            userId,
            name,
            AccountType.Card,
            "RUB",
            Money.Zero("RUB"),
            createdAt);
    }

    private static DbContextOptions<MoneyFlowDbContext> CreateOptions()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONEYFLOW_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set MONEYFLOW_TEST_CONNECTION_STRING before running PostgreSQL tests.");
        
        var connection = new NpgsqlConnectionStringBuilder(connectionString);
        if (connection.Database != "moneyflow_issue12_tests")
            throw new InvalidOperationException("PostgreSQL tests must use the moneyflow_issue12_tests database.");

        return new DbContextOptionsBuilder<MoneyFlowDbContext>().UseNpgsql(connectionString).Options;
    }
}