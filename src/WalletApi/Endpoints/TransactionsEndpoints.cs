using Microsoft.AspNetCore.Mvc;
using WalletApi.Services;
using WalletApi.Helpers;

namespace WalletApi.Endpoints;

public static class TransactionsEndpoints
{
    public static void MapTransactionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions")
                       .WithTags("Transactions")
                       .WithOpenApi()
                       .RequireAuthorization();

        // GET /api/transactions/{uuid}
        group.MapGet("/{uuid:guid}", async (
            Guid uuid,
            [FromServices] ITransactionService txSvc,
            CancellationToken ct) =>
        {
            var tx = await txSvc.GetByUuidAsync(uuid, ct);
            return ApiResults.Ok(tx, "Transaction retrieved successfully.");
        })
        .WithSummary("Get a transaction by UUID.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/transactions/{uuid}/confirm
        group.MapPost("/{uuid:guid}/confirm", async (
            Guid uuid,
            [FromServices] ITransactionService txSvc,
            CancellationToken ct) =>
        {
            var tx = await txSvc.ConfirmAsync(uuid, ct);
            return ApiResults.Ok(tx, "Transaction confirmed successfully.");
        })
        .WithSummary("Confirm a pending (unconfirmed) transaction — applies the balance change.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/transactions/{uuid}/revert
        group.MapPost("/{uuid:guid}/revert", async (
            Guid uuid,
            [FromServices] ITransactionService txSvc,
            CancellationToken ct) =>
        {
            var tx = await txSvc.RevertAsync(uuid, ct);
            return ApiResults.Ok(tx, "Transaction reverted successfully.");
        })
        .WithSummary("Revert a confirmed transaction — reverses the balance change.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
