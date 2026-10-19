using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Application.Accounts;

public sealed record AccountResultDTO(
    Guid Id,
    Guid UserId,
    string Name,
    AccountType Type,
    string CurrencyCode,
    decimal OpeningBalance,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ArchivedAt);