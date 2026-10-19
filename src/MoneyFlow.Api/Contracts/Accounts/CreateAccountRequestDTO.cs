namespace MoneyFlow.Api.Contracts.Accounts;

public sealed record CreateAccountRequestDTO(
    Guid UserId,
    string Name,
    string Type,
    string CurrencyCode,
    decimal OpeningBalance);