using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MoneyFlow.Api.ErrorHandling;
using MoneyFlow.Application.Common.Exceptions;

namespace MoneyFlow.Api.Tests;

public sealed class ProblemDetailsTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task UnknownRoute_ReturnsNotFoundProblemDetails()
    {
        var response = await _client.GetAsync("/does-not-exist");
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    [Fact]
    public async Task UnexpectedException_ReturnsSafeInternalServerError()
    {
        using var scope = _factory.Services.CreateScope();

        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);

        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };

        context.Request.Path = "/errors/test";
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("Secret details"), CancellationToken.None);

        context.Response.Body.Position = 0;
        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        Assert.NotNull(problemDetails);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal("An unexpected error occurred.", problemDetails.Detail);

        Assert.Equal("/errors/test", problemDetails.Instance);
        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    [Theory]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest, "Bad Request")]
    [InlineData(typeof(NotFoundException), StatusCodes.Status404NotFound, "Not Found")]
    [InlineData(typeof(ConflictException), StatusCodes.Status409Conflict, "Conflict")]
    public async Task ExpectedException_ReturnsMappedProblemDetails(Type exceptionType, int expectedStatus, string expectedTitle)
    {
        using var scope = _factory.Services.CreateScope();

        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);

        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        context.Request.Path = "/errors/test";
        context.Response.Body = new MemoryStream();

        var exception = (Exception)Activator.CreateInstance(exceptionType, "Expected details")!;
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Position = 0;

        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.True(handled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);

        Assert.NotNull(problemDetails);
        Assert.Equal(expectedTitle, problemDetails.Title);
        Assert.Equal(expectedStatus, problemDetails.Status);
        Assert.Equal("Expected details", problemDetails.Detail);
        Assert.Equal("/errors/test", problemDetails.Instance);
        Assert.Equal("application/problem+json", context.Response.ContentType);
    }
}