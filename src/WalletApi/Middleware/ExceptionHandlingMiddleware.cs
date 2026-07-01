using WalletApi.Domain.Exceptions;

namespace WalletApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);

            if (!context.Response.HasStarted)
            {
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, "Unauthorized access (missing or invalid token).");
                }
                else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    await WriteErrorResponseAsync(context, StatusCodes.Status403Forbidden, "Forbidden (you do not have permission to access this resource).");
                }
                else if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, "The requested resource was not found.");
                }
                else if (context.Response.StatusCode == StatusCodes.Status405MethodNotAllowed)
                {
                    await WriteErrorResponseAsync(context, StatusCodes.Status405MethodNotAllowed, "HTTP method not allowed.");
                }
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            AmountInvalidException      => (StatusCodes.Status400BadRequest,   ex.Message),
            InvalidOperationException   => (StatusCodes.Status400BadRequest,   ex.Message),
            BalanceIsEmptyException     => (StatusCodes.Status422UnprocessableEntity, ex.Message),
            InsufficientFundsException  => (StatusCodes.Status422UnprocessableEntity, ex.Message),
            KeyNotFoundException        => (StatusCodes.Status404NotFound,     ex.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access."),
            _                           => (StatusCodes.Status500InternalServerError, "An internal server error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        }
        else
        {
            logger.LogWarning(ex, "Domain exception intercepted: {Message}", ex.Message);
        }

        await WriteErrorResponseAsync(context, statusCode, message);
    }

    private static Task WriteErrorResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = message,
            data = (object?)null
        });
    }
}
