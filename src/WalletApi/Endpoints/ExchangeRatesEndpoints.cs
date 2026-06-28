using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;

namespace WalletApi.Endpoints;

public static class ExchangeRatesEndpoints
{
    public static void MapExchangeRatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rates")
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
            return Results.CreatedAtRoute("GetExchangeRate",
                new { from = response.FromCurrency, to = response.ToCurrency },
                response);
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
            return Results.Ok(rates);
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
            return Results.Ok(rate);
        })
        .WithName("GetExchangeRate")
        .WithSummary("Get the latest exchange rate for a specific currency pair.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
