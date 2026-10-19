using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Entities;

namespace MoneyFlow.Domain.Services;

public static class AccountBalanceCalculator
{
    public static Money Calculate(Account account, IEnumerable<Transaction> transactions, DateTimeOffset? asOf =  null)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(transactions);

        return account.OpeningBalance;
    }
}