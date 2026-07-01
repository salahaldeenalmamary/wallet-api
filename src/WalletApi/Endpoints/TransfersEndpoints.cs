using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;
using WalletApi.Helpers;

namespace WalletApi.Endpoints;

public static class TransfersEndpoints
{
    public static void MapTransfersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transfers")
                       .WithTags("Transfers")
                       .WithOpenApi()
                       .RequireAuthorization();

        // POST /api/transfers
        group.MapPost("/", async (
            [FromBody] TransferRequest request,
            [FromServices] ITransferService transferSvc,
            CancellationToken ct) =>
        {
            var transfer = await transferSvc.TransferAsync(request, ct);
            return ApiResults.Ok(transfer, "Transfer completed successfully.");
        })
        .WithSummary("Transfer funds between two wallets (fails on insufficient balance).")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/transfers/force
        group.MapPost("/force", async (
            [FromBody] TransferRequest request,
            [FromServices] ITransferService transferSvc,
            CancellationToken ct) =>
        {
            var transfer = await transferSvc.ForceTransferAsync(request, ct);
            return ApiResults.Ok(transfer, "Force transfer completed successfully.");
        })
        .WithSummary("Force-transfer funds (ignores balance check).")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/transfers/safe
        group.MapPost("/safe", async (
            [FromBody] TransferRequest request,
            [FromServices] ITransferService transferSvc,
            CancellationToken ct) =>
        {
            var transfer = await transferSvc.SafeTransferAsync(request, ct);
            return transfer is null ? ApiResults.NoContent("Transfer skipped due to insufficient funds.") : ApiResults.Ok(transfer, "Safe transfer completed successfully.");
        })
        .WithSummary("Safe transfer — returns 204 on insufficient funds instead of throwing.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent);

        // GET /api/transfers/{uuid}
        group.MapGet("/{uuid:guid}", async (
            Guid uuid,
            [FromServices] ITransferService transferSvc,
            CancellationToken ct) =>
        {
            var transfer = await transferSvc.GetByUuidAsync(uuid, ct);
            return ApiResults.Ok(transfer, "Transfer retrieved successfully.");
        })
        .WithSummary("Get a transfer by UUID.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
