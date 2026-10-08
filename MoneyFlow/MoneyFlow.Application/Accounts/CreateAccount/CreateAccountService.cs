using MoneyFlow.Application.Common.Exceptions;
using MoneyFlow.Application.Messaging;
using MoneyFlow.Contracts.Events;
using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Entities;

namespace MoneyFlow.Application.Accounts.CreateAccount;

public sealed class CreateAccountService(IAccountStore accountStore, TimeProvider timeProvider, IAccountCreatedPublisher publisher)
{
    public async Task<AccountResultDTO> ExecuteAsync(CreateAccountInputDTO input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var openingBalance = Money.Of(input.OpeningBalance, input.CurrencyCode);
        var account = new Account(
            Guid.NewGuid(),
            input.UserId,
            input.Name,
            input.Type,
            input.CurrencyCode,
            openingBalance,
            timeProvider.GetUtcNow());

        var userExists = await accountStore.UserExistsAsync(account.UserId, cancellationToken);
        if (!userExists)
            throw new NotFoundException($"User '{account.UserId}' was not found.");

        var nameExists = await accountStore.NameExistsAsync(account.UserId, account.Name, cancellationToken);
        if (nameExists)
            throw new ConflictException($"Account named '{account.Name}' already exists.");
        
        await accountStore.AddAsync(account, cancellationToken);
        var accountCreated = new AccountCreated(Guid.NewGuid(), account.Id, account.UserId, account.CreatedAt);
        await publisher.PublishAsync(accountCreated, cancellationToken);
        return new AccountResultDTO(
            account.Id,
            account.UserId,
            account.Name,
            account.Type,
            account.CurrencyCode,
            account.OpeningBalance.Amount,
            account.IsArchived,
            account.CreatedAt,
            account.ArchivedAt);
    }
}