using Microsoft.AspNetCore.Mvc;
using WalletApi.Services;

namespace WalletApi.Controllers;

[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionsController(ITransactionService txSvc) : ControllerBase
{
    /// <summary>Get a transaction by UUID.</summary>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid uuid, CancellationToken ct)
    {
        var tx = await txSvc.GetByUuidAsync(uuid, ct);
        return Ok(tx);
    }

    /// <summary>Confirm a pending (unconfirmed) transaction — applies the balance change.</summary>
    [HttpPost("{uuid:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid uuid, CancellationToken ct)
    {
        var tx = await txSvc.ConfirmAsync(uuid, ct);
        return Ok(tx);
    }

    /// <summary>Revert a confirmed transaction — reverses the balance change.</summary>
    [HttpPost("{uuid:guid}/revert")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revert(Guid uuid, CancellationToken ct)
    {
        var tx = await txSvc.RevertAsync(uuid, ct);
        return Ok(tx);
    }
}
