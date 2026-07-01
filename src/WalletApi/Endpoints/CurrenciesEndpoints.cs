using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;
using WalletApi.Helpers;

namespace WalletApi.Endpoints;

public static class CurrenciesEndpoints
{
    public static void MapCurrenciesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/currencies")
                       .WithTags("Currencies")
                       .WithOpenApi()
                       .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateCurrencyRequest request, 
            [FromServices] ICurrencyService currencyService, 
            CancellationToken ct) =>
        {
            var response = await currencyService.CreateAsync(request, ct);
            return ApiResults.Created($"/currencies/{response.Code}", response, "Currency created successfully.");
        })
        .WithSummary("Create a new currency.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/", async (
            [FromServices] ICurrencyService currencyService, 
            CancellationToken ct) =>
        {
            var currencies = await currencyService.ListAsync(ct);
            return ApiResults.Ok(currencies, "Currencies retrieved successfully.");
        })
        .WithSummary("List all active currencies.")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{code}", async (
            string code, 
            [FromServices] ICurrencyService currencyService, 
            CancellationToken ct) =>
        {
            var currency = await currencyService.GetAsync(code, ct);
            return ApiResults.Ok(currency, "Currency retrieved successfully.");
        })
        .WithName("GetCurrency")
        .WithSummary("Get a specific currency by ISO code.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
