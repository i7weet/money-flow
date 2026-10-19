using Microsoft.AspNetCore.Diagnostics;
using MoneyFlow.Application.Common.Exceptions;

namespace MoneyFlow.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {

        var (statusCode, title, detail) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Request failed for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            instance: httpContext.Request.Path)
            .ExecuteAsync(httpContext);

        return true;
    }
}