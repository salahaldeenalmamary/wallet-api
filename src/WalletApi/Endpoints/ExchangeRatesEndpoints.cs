using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;
using WalletApi.Helpers;

namespace WalletApi.Endpoints;

public static class ExchangeRatesEndpoints
{
    public static void MapExchangeRatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/rates")
                       .WithTags("Rates")
                       .WithOpenApi()
                       .RequireAuthorization();

        // POST /api/rates
        group.MapPost("/", async (
            [FromBody] SetExchangeRateRequest request,
            [FromServices] IExchangeRateService exchangeRateService,
            CancellationToken ct) =>
        {
            var response = await exchangeRateService.SetRateAsync(request, ct);
            return ApiResults.Created($"/rates/{response.FromCurrency}/{response.ToCurrency}", response, "Exchange rate set successfully.");
        })
        .WithSummary("Set (or update) the exchange rate for a currency pair.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/rates
        group.MapGet("/", async (
            [FromServices] IExchangeRateService exchangeRateService,
            CancellationToken ct) =>
        {
            var rates = await exchangeRateService.ListAsync(ct);
            return ApiResults.Ok(rates, "Exchange rates retrieved successfully.");
        })
        .WithSummary("List the latest exchange rate for each currency pair.")
        .Produces(StatusCodes.Status200OK);

        // GET /api/rates/{from}/{to}
        group.MapGet("/{from}/{to}", async (
            string from,
            string to,
            [FromServices] IExchangeRateService exchangeRateService,
            CancellationToken ct) =>
        {
            var rate = await exchangeRateService.GetRateAsync(from, to, ct);
            return ApiResults.Ok(rate, "Exchange rate retrieved successfully.");
        })
        .WithName("GetExchangeRate")
        .WithSummary("Get the latest exchange rate for a specific currency pair.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
