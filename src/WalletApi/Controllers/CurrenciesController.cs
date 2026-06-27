using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;

namespace WalletApi.Controllers;

/// <summary>Manage supported currencies.</summary>
[ApiController]
[Route("currencies")]
[Produces("application/json")]
public class CurrenciesController(ICurrencyService currencyService) : ControllerBase
{
    /// <summary>Create a new currency.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCurrencyRequest request, CancellationToken ct)
    {
        var response = await currencyService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { code = response.Code }, response);
    }

    /// <summary>List all active currencies.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var currencies = await currencyService.ListAsync(ct);
        return Ok(currencies);
    }

    /// <summary>Get a specific currency by ISO code.</summary>
    [HttpGet("{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string code, CancellationToken ct)
    {
        var currency = await currencyService.GetAsync(code, ct);
        return Ok(currency);
    }
}
