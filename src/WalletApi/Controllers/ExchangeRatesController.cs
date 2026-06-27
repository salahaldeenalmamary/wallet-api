using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;

namespace WalletApi.Controllers;

/// <summary>Manage currency exchange rates.</summary>
[ApiController]
[Route("exchange-rates")]
[Produces("application/json")]
public class ExchangeRatesController(IExchangeRateService exchangeRateService) : ControllerBase
{
    /// <summary>
    /// Set (or update) the exchange rate for a currency pair.
    /// A new record is always inserted to preserve history; the latest record is used for conversions.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Set(
        [FromBody] SetExchangeRateRequest request, CancellationToken ct)
    {
        var response = await exchangeRateService.SetRateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { from = response.FromCurrency, to = response.ToCurrency }, response);
    }

    /// <summary>List the latest exchange rate for each currency pair.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rates = await exchangeRateService.ListAsync(ct);
        return Ok(rates);
    }

    /// <summary>Get the latest exchange rate for a specific currency pair.</summary>
    /// <param name="from">Source currency ISO code, e.g. USD</param>
    /// <param name="to">Target currency ISO code, e.g. EUR</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{from}/{to}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string from, string to, CancellationToken ct)
    {
        var rate = await exchangeRateService.GetRateAsync(from, to, ct);
        return Ok(rate);
    }
}
