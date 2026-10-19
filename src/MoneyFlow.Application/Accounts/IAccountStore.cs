using MoneyFlow.Domain.Entities;

namespace MoneyFlow.Application.Accounts;

public interface IAccountStore
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(Guid userId, string name, CancellationToken cancellationToken);
    Task<Account?> FindByIdAsync(Guid accountId, CancellationToken cancellationToken);
    Task AddAsync(Account account, CancellationToken cancellationToken);
}