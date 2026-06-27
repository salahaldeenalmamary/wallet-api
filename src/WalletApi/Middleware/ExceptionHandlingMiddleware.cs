using WalletApi.Domain.Exceptions;

namespace WalletApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, error) = ex switch
        {
            AmountInvalidException      => (StatusCodes.Status400BadRequest,   "amount_invalid"),
            BalanceIsEmptyException     => (StatusCodes.Status422UnprocessableEntity, "balance_empty"),
            InsufficientFundsException  => (StatusCodes.Status422UnprocessableEntity, "insufficient_funds"),
            KeyNotFoundException        => (StatusCodes.Status404NotFound,     "not_found"),
            _                          => (StatusCodes.Status500InternalServerError, "internal_error")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(new
        {
            error,
            message = ex.Message
        });
    }
}
