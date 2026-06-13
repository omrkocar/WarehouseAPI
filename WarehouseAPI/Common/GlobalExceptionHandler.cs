using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace WarehouseAPI.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        var (statusCode, message) = exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                "The resource was modified by another request. Please try again."),
            _ => (StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.")
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}