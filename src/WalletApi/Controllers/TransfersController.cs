using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;

namespace WalletApi.Controllers;

[ApiController]
[Route("api/transfers")]
[Produces("application/json")]
public class TransfersController(ITransferService transferSvc) : ControllerBase
{
    /// <summary>Transfer funds between two wallets (fails on insufficient balance).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request, CancellationToken ct)
    {
        var transfer = await transferSvc.TransferAsync(request, ct);
        return Ok(transfer);
    }

    /// <summary>Force-transfer funds (ignores balance check).</summary>
    [HttpPost("force")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForceTransfer([FromBody] TransferRequest request, CancellationToken ct)
    {
        var transfer = await transferSvc.ForceTransferAsync(request, ct);
        return Ok(transfer);
    }

    /// <summary>Safe transfer — returns null body (204) on insufficient funds instead of throwing.</summary>
    [HttpPost("safe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SafeTransfer([FromBody] TransferRequest request, CancellationToken ct)
    {
        var transfer = await transferSvc.SafeTransferAsync(request, ct);
        return transfer is null ? NoContent() : Ok(transfer);
    }

    /// <summary>Get a transfer by UUID.</summary>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid uuid, CancellationToken ct)
    {
        var transfer = await transferSvc.GetByUuidAsync(uuid, ct);
        return Ok(transfer);
    }
}
