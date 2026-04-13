using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace Loggy.Api.ErrorHandling;

/// <summary>Error handling catch-all & return consistently formatted error JSON to consumer</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellation)
    {
        var (status, message) = exception switch
        {
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
            JsonException => (StatusCodes.Status400BadRequest, "Invalid JSON format"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        logger.LogError("Error {Message}", exception.Message);

        httpContext.Response.StatusCode = status;
        // Logging request id (or similar identifier) would be nice to add
        await httpContext.Response.WriteAsJsonAsync(new { message, statusCode = status });

        return true;
    }
}

