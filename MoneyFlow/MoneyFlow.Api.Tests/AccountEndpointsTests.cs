using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoneyFlow.Api.Contracts.Accounts;
using MoneyFlow.Application.Accounts;
using MoneyFlow.Domain.Entities;

namespace MoneyFlow.Api.Tests;

public sealed class AccountEndpointsTests
{
    [Fact]
    public async Task CreateAccount_ValidRequest_ReturnsCreatedAndReadableLocation()
    {
        var userId = Guid.NewGuid();
        using var factory = CreateFactory(userId);
        using var client = factory.CreateClient();
        var request = new CreateAccountRequestDTO(userId, "  Main card  ", "card", "rub", 1000.12345m);
        using var createResponse = await client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal("application/json", createResponse.Content.Headers.ContentType?.MediaType);

        var created = await createResponse.Content.ReadFromJsonAsync<AccountResponseDTO>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(userId, created.UserId);
        Assert.Equal("Main card", created.Name);
        Assert.Equal("Card", created.Type);
        Assert.Equal("RUB", created.CurrencyCode);
        Assert.Equal(1000.1235m, created.OpeningBalance);
        Assert.False(created.IsArchived);
        Assert.Null(created.ArchivedAt);

        var location = createResponse.Headers.Location;

        Assert.NotNull(location);

        var locationUri = new Uri(client.BaseAddress!, location);

        Assert.Equal($"/api/accounts/{created.Id}", locationUri.AbsolutePath);

        using var getResponse = await client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<AccountResponseDTO>();

        Assert.NotNull(fetched);
        Assert.Equal(created, fetched);
    }

    [Theory]
    [InlineData("Main card", "Unknown", "RUB")]
    [InlineData("Main card", "2", "RUB")]
    [InlineData("Main card", "Cash, Card", "RUB")]
    [InlineData("Main card", "", "RUB")]
    [InlineData("Main card", null, "RUB")]
    [InlineData("   ", "Card", "RUB")]
    [InlineData("Main card", "Card", "R1")]
    public async Task CreateAccount_InvalidRequest_ReturnsBadRequest(string name, string? type, string currencyCode)
    {
        var userId = Guid.NewGuid();
        using var factory = CreateFactory(userId);
        using var client = factory.CreateClient();
        var request = new CreateAccountRequestDTO(userId, name, type!, currencyCode, 100m);
        using var response = await client.PostAsJsonAsync("/api/accounts", request);
      
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Bad Request");
    }

    [Fact]
    public async Task CreateAccount_MissingUser_ReturnsNotFound()
    {
        using var factory = CreateFactory(Guid.NewGuid());
        using var client = factory.CreateClient();
        var request = new CreateAccountRequestDTO(Guid.NewGuid(), "Main card", "Card", "RUB", 100m);
        using var response = await client.PostAsJsonAsync("/api/accounts", request);
       
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Not Found");
    }

    [Fact]
    public async Task CreateAccount_DuplicateName_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        using var factory = CreateFactory(userId);
        using var client = factory.CreateClient();
        var request = new CreateAccountRequestDTO(userId, "Main card", "Card", "RUB", 100m);
        using var firstResponse = await client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var duplicateResponse = await client.PostAsJsonAsync("/api/accounts", request with { Name = "  Main card  " });

        await AssertProblemAsync(duplicateResponse, HttpStatusCode.Conflict, "Conflict");
    }

    [Fact]
    public async Task GetAccount_MissingAccount_ReturnsNotFound()
    {
        using var factory = CreateFactory(Guid.NewGuid());
        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"/api/accounts/{Guid.NewGuid()}");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Not Found");
    }

    [Fact]
    public async Task GetAccount_EmptyId_ReturnsBadRequest()
    {
        using var factory = CreateFactory(Guid.NewGuid());
        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"/api/accounts/{Guid.Empty}");

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Bad Request");
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expectedStatus, string expectedTitle)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal((int)expectedStatus, problem.Status);
        Assert.Equal(expectedTitle, problem.Title);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }

    private sealed class AccountStoreFake(Guid existingUserId) : IAccountStore
    {
        private readonly Dictionary<Guid, Account> _accounts = [];

        public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(userId == existingUserId);
        }

        public Task<bool> NameExistsAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            return Task.FromResult(_accounts.Values.Any(account => account.UserId == userId && account.Name == name));
        }

        public Task<Account?> FindByIdAsync(Guid accountId, CancellationToken cancellationToken)
        {
            _accounts.TryGetValue(accountId, out var account);
            return Task.FromResult(account);
        }

        public Task AddAsync(Account account, CancellationToken cancellationToken)
        {
            _accounts.Add(account.Id, account);
            return Task.CompletedTask;
        }
    }

    private static WebApplicationFactory<Program> CreateFactory(Guid existingUserId)
    {
        var store = new AccountStoreFake(existingUserId);

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IAccountStore>();
                    services.AddSingleton<IAccountStore>(store);
                });
            });
    }
}