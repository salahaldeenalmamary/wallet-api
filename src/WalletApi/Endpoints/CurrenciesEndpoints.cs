using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Requests;
using WalletApi.Services;

namespace WalletApi.Endpoints;

public static class CurrenciesEndpoints
{
    public static void MapCurrenciesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/currencies")
                       .WithTags("Currencies")
                       .WithOpenApi()
                       .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateCurrencyRequest request, 
            [FromServices] ICurrencyService currencyService, 
            CancellationToken ct) =>
        {
            var response = await currencyService.CreateAsync(request, ct);
            return Results.CreatedAtRoute("GetCurrency", new { code = response.Code }, response);
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
            return Results.Ok(currencies);
        })
        .WithSummary("List all active currencies.")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{code}", async (
            string code, 
            [FromServices] ICurrencyService currencyService, 
            CancellationToken ct) =>
        {
            var currency = await currencyService.GetAsync(code, ct);
            return Results.Ok(currency);
        })
        .WithName("GetCurrency")
        .WithSummary("Get a specific currency by ISO code.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
