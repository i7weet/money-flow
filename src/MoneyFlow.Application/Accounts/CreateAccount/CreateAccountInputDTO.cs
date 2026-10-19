using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Application.Accounts.CreateAccount;

public sealed record CreateAccountInputDTO(
    Guid UserId,
    string Name,
    AccountType Type,
    string CurrencyCode,
    decimal OpeningBalance);