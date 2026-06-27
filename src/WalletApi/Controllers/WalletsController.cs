using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;

namespace WalletApi.Controllers;

[ApiController]
[Route("api/wallets")]
[Produces("application/json")]
public class WalletsController(IWalletService walletSvc, ITransactionService txSvc) : ControllerBase
{
    /// <summary>Create a new wallet for a holder.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWalletRequest request, CancellationToken ct)
    {
        var wallet = await walletSvc.CreateWalletAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = wallet.Id }, wallet);
    }

    /// <summary>Get a wallet by ID.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var wallet = await walletSvc.GetWalletAsync(id, ct);
        return Ok(wallet);
    }

    /// <summary>List transactions for a wallet (paged).</summary>
    [HttpGet("{id:long}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Transactions(
        long id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await walletSvc.GetTransactionsAsync(id, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>List transfers for a wallet (paged).</summary>
    [HttpGet("{id:long}/transfers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Transfers(
        long id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await walletSvc.GetTransfersAsync(id, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Deposit funds into a wallet.</summary>
    [HttpPost("{id:long}/deposit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit(long id, [FromBody] DepositRequest request, CancellationToken ct)
    {
        var tx = await txSvc.DepositAsync(id, request, ct);
        return Ok(tx);
    }

    /// <summary>Withdraw funds from a wallet (fails if insufficient balance).</summary>
    [HttpPost("{id:long}/withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw(long id, [FromBody] WithdrawRequest request, CancellationToken ct)
    {
        var tx = await txSvc.WithdrawAsync(id, request, ct);
        return Ok(tx);
    }

    /// <summary>Force-withdraw funds (ignores balance check — balance can go negative).</summary>
    [HttpPost("{id:long}/force-withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForceWithdraw(long id, [FromBody] WithdrawRequest request, CancellationToken ct)
    {
        var tx = await txSvc.ForceWithdrawAsync(id, request, ct);
        return Ok(tx);
    }

    /// <summary>Check whether a wallet can withdraw a given amount.</summary>
    [HttpGet("{id:long}/can-withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CanWithdraw(
        long id, [FromQuery] decimal amount, [FromQuery] bool allowZero = false, CancellationToken ct = default)
    {
        var result = await txSvc.CanWithdrawAsync(id, amount, allowZero, ct);
        return Ok(new { canWithdraw = result });
    }

    /// <summary>Recompute and sync the wallet balance from confirmed transactions.</summary>
    [HttpPost("{id:long}/refresh-balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefreshBalance(long id, CancellationToken ct)
    {
        var wallet = await walletSvc.RefreshBalanceAsync(id, ct);
        return Ok(wallet);
    }
}
