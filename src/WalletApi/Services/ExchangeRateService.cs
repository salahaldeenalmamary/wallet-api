using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;
using WalletApi.Entities;

namespace WalletApi.Services;

public class ExchangeRateService(WalletDbContext db) : IExchangeRateService
{
    public async Task<ExchangeRateResponse> SetRateAsync(SetExchangeRateRequest request, CancellationToken ct = default)
    {
        var from = request.FromCurrency.ToUpperInvariant();
        var to = request.ToCurrency.ToUpperInvariant();

        if (from == to)
            throw new InvalidOperationException("From and To currencies must be different.");

        // Validate both currencies exist
        if (!await db.Currencies.AnyAsync(c => c.Code == from && c.IsActive, ct))
            throw new KeyNotFoundException($"Currency '{from}' not found or inactive.");
        if (!await db.Currencies.AnyAsync(c => c.Code == to && c.IsActive, ct))
            throw new KeyNotFoundException($"Currency '{to}' not found or inactive.");

        // Always insert a new record to preserve history; latest record is used for lookups
        var rate = new ExchangeRate
        {
            FromCurrency = from,
            ToCurrency = to,
            Rate = request.Rate,
            CreatedAt = DateTime.UtcNow
        };

        db.ExchangeRates.Add(rate);
        await db.SaveChangesAsync(ct);

        return ToResponse(rate);
    }

    public async Task<ExchangeRateResponse> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken ct = default)
    {
        var from = fromCurrency.ToUpperInvariant();
        var to = toCurrency.ToUpperInvariant();

        var rate = await db.ExchangeRates
            .Where(r => r.FromCurrency == from && r.ToCurrency == to)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"No exchange rate found for {from} → {to}.");

        return ToResponse(rate);
    }

    public async Task<IReadOnlyList<ExchangeRateResponse>> ListAsync(CancellationToken ct = default)
    {
        // Return only the latest rate per pair
        var rates = await db.ExchangeRates
            .GroupBy(r => new { r.FromCurrency, r.ToCurrency })
            .Select(g => g.OrderByDescending(r => r.CreatedAt).First())
            .ToListAsync(ct);

        return rates.Select(ToResponse).ToList();
    }

    public async Task<decimal> ConvertAsync(
        decimal amount, string fromCurrency, string toCurrency, CancellationToken ct = default)
    {
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            return amount;

        var rateResponse = await GetRateAsync(fromCurrency, toCurrency, ct);
        return amount * rateResponse.Rate;
    }

    // ── Helper ───────────────────────────────────────────────────────────

    private static ExchangeRateResponse ToResponse(ExchangeRate r) =>
        new(r.Id, r.FromCurrency, r.ToCurrency, r.Rate, r.CreatedAt);
}
