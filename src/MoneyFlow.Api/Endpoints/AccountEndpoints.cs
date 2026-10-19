using MoneyFlow.Api.Contracts.Accounts;
using MoneyFlow.Application.Accounts.CreateAccount;
using MoneyFlow.Application.Accounts.GetAccount;
using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Api.Endpoints;

public static class AccountEndpoints
{
    public const string GetAccountByIdRouteName = "GetAccountById";

    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapGet(
                "/{accountId:guid}",
                async (Guid accountId, GetAccountService service, CancellationToken token) =>
                {
                    var account = await service.ExecuteAsync(accountId, token);
                    var response = new AccountResponseDTO(
                        account.Id,
                        account.UserId,
                        account.Name,
                        account.Type.ToString(),
                        account.CurrencyCode,
                        account.OpeningBalance,
                        account.IsArchived,
                        account.CreatedAt,
                        account.ArchivedAt);
                    return TypedResults.Ok(response); 
                }
                )
            .WithName(GetAccountByIdRouteName)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/",
            async (CreateAccountRequestDTO requestDTO, CreateAccountService service, CancellationToken token) =>
            {
                var normalizedType = requestDTO.Type?.Trim();

                if (!Enum.TryParse<AccountType>(normalizedType, ignoreCase: true, out var accountType) ||
                    !Enum.IsDefined(accountType) ||
                    !string.Equals(accountType.ToString(), normalizedType , StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Account type must be Cash, Card, Bank, Savings or Investment",
                        nameof(requestDTO.Type));
                }
                
                var input = new CreateAccountInputDTO(
                    requestDTO.UserId,
                    requestDTO.Name,
                    accountType,
                    requestDTO.CurrencyCode,
                    requestDTO.OpeningBalance);

                var account = await service.ExecuteAsync(input, token);
                
                var response = new AccountResponseDTO(
                    account.Id,
                    account.UserId,
                    account.Name,
                    account.Type.ToString(),
                    account.CurrencyCode,
                    account.OpeningBalance,
                    account.IsArchived,
                    account.CreatedAt,
                    account.ArchivedAt);

                return TypedResults.CreatedAtRoute(
                    value: response,
                    routeName: GetAccountByIdRouteName,
                    routeValues: new { accountId = response.Id });
            })
            .WithName("CreateAccount")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        
        return endpoints;
    }
}