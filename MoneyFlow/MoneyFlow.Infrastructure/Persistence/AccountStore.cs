using Microsoft.EntityFrameworkCore;
using MoneyFlow.Application.Accounts;
using MoneyFlow.Application.Common.Exceptions;
using MoneyFlow.Domain.Entities;
using Npgsql;

namespace MoneyFlow.Infrastructure.Persistence;

public sealed class AccountStore(MoneyFlowDbContext dbContext) : IAccountStore
{
    private const string UniqueAccountNameIndex = "IX_accounts_UserId_Name";
    
    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(x => x.Id == userId, cancellationToken);
    }

    public Task<bool> NameExistsAsync(Guid userId, string name, CancellationToken cancellationToken)
    {
        return dbContext.Accounts.AnyAsync(x => x.UserId == userId && x.Name == name, cancellationToken);
    }

    public Task<Account?> FindByIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return dbContext.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);
        dbContext.Accounts.Add(account);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
                  {
                      SqlState: PostgresErrorCodes.UniqueViolation,
                      ConstraintName: UniqueAccountNameIndex
                  })
        {
            throw new ConflictException(
                $"Account named '{account.Name}' already exists.");
        }
    }
}