namespace MoneyFlow.Api.Contracts.Accounts;

public sealed record AccountResponseDTO(
    Guid Id,
    Guid UserId,
    string Name,
    string Type,
    string CurrencyCode,
    decimal OpeningBalance,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ArchivedAt);