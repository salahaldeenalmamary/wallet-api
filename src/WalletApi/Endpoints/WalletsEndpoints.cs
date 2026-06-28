using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;

namespace WalletApi.Endpoints;

public static class WalletsEndpoints
{
    public static void MapWalletsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wallets")
                       .WithTags("Wallets")
                       .WithOpenApi()
                       .RequireAuthorization();

        // POST /api/wallets
        group.MapPost("/", async (
            [FromBody] CreateWalletRequest request,
            [FromServices] IWalletService walletSvc,
            CancellationToken ct) =>
        {
            var wallet = await walletSvc.CreateWalletAsync(request, ct);
            return Results.CreatedAtRoute("GetWallet", new { id = wallet.Id }, wallet);
        })
        .WithSummary("Create a new wallet for a holder.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // GET /api/wallets/{id}
        group.MapGet("/{id:long}", async (
            long id,
            [FromServices] IWalletService walletSvc,
            CancellationToken ct) =>
        {
            var wallet = await walletSvc.GetWalletAsync(id, ct);
            return Results.Ok(wallet);
        })
        .WithName("GetWallet")
        .WithSummary("Get a wallet by ID.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/wallets/{id}/transactions
        group.MapGet("/{id:long}/transactions", async (
            long id,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] IWalletService walletSvc,
            CancellationToken ct) =>
        {
            var result = await walletSvc.GetTransactionsAsync(id, page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize, ct);
            return Results.Ok(result);
        })
        .WithSummary("List transactions for a wallet (paged).")
        .Produces(StatusCodes.Status200OK);

        // GET /api/wallets/{id}/transfers
        group.MapGet("/{id:long}/transfers", async (
            long id,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] IWalletService walletSvc,
            CancellationToken ct) =>
        {
            var result = await walletSvc.GetTransfersAsync(id, page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize, ct);
            return Results.Ok(result);
        })
        .WithSummary("List transfers for a wallet (paged).")
        .Produces(StatusCodes.Status200OK);

        // POST /api/wallets/{id}/deposit
        group.MapPost("/{id:long}/deposit", async (
            long id,
            [FromBody] DepositRequest request,
            [FromServices] ITransactionService txSvc,
            CancellationToken ct) =>
        {
            var tx = await txSvc.DepositAsync(id, request, ct);
            return Results.Ok(tx);
        })
        .WithSummary("Deposit funds into a wallet.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/wallets/{id}/withdraw
        group.MapPost("/{id:long}/withdraw", async (
            long id,
            [FromBody] WithdrawRequest request,
            [FromServices] ITransactionService txSvc,
            CancellationToken ct) =>
        {
            var tx = await txSvc.WithdrawAsync(id, request, ct);
            return Results.Ok(tx);
        })
        .WithSummary("Withdraw funds from a wallet (fails if insufficient balance).")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/wallets/{id}/force-withdraw
        group.MapPost("/{id:long}/force-withdraw", async (
            long id,
            [FromBody] WithdrawRequest request,
            [FromServices] ITransactionService txSvc,
            CancellationToken ct) =>
        {
            var tx = await txSvc.ForceWithdrawAsync(id, request, ct);
            return Results.Ok(tx);
        })
        .WithSummary("Force-withdraw funds (ignores balance check — balance can go negative).")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/wallets/{id}/can-withdraw
        group.MapGet("/{id:long}/can-withdraw", async (
            long id,
            [FromQuery] decimal amount,
            [FromQuery] bool allowZero,
            [FromServices] ITransactionService txSvc,
            CancellationToken ct) =>
        {
            var result = await txSvc.CanWithdrawAsync(id, amount, allowZero, ct);
            return Results.Ok(new { canWithdraw = result });
        })
        .WithSummary("Check whether a wallet can withdraw a given amount.")
        .Produces(StatusCodes.Status200OK);

        // POST /api/wallets/{id}/refresh-balance
        group.MapPost("/{id:long}/refresh-balance", async (
            long id,
            [FromServices] IWalletService walletSvc,
            CancellationToken ct) =>
        {
            var wallet = await walletSvc.RefreshBalanceAsync(id, ct);
            return Results.Ok(wallet);
        })
        .WithSummary("Recompute and sync the wallet balance from confirmed transactions.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
