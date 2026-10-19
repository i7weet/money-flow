using MoneyFlow.Application.Common.Exceptions;
using MoneyFlow.Domain.Entities;

namespace MoneyFlow.Application.Accounts.GetAccount;

public class GetAccountService(IAccountStore accountStore)
{
    public async Task<AccountResultDTO> ExecuteAsync(Guid accountId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (accountId == Guid.Empty)
            throw new ArgumentException("Account id cannot be empty", nameof(accountId));
        
        var account = await accountStore.FindByIdAsync(accountId, cancellationToken);
        if (account is null)
            throw new NotFoundException($"Account '{accountId}' was not found.");

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