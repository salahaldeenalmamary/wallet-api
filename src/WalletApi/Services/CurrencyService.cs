using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;
using WalletApi.Entities;

namespace WalletApi.Services;

public class CurrencyService(WalletDbContext db) : ICurrencyService
{
    public async Task<CurrencyResponse> CreateAsync(CreateCurrencyRequest request, CancellationToken ct = default)
    {
        var code = request.Code.ToUpperInvariant();

        if (await db.Currencies.AnyAsync(c => c.Code == code, ct))
            throw new InvalidOperationException($"Currency '{code}' already exists.");

        var currency = new Currency
        {
            Code = code,
            Name = request.Name,
            Symbol = request.Symbol,
            DecimalPlaces = request.DecimalPlaces,
            IsActive = true
        };

        db.Currencies.Add(currency);
        await db.SaveChangesAsync(ct);

        return ToResponse(currency);
    }

    public async Task<IReadOnlyList<CurrencyResponse>> ListAsync(CancellationToken ct = default)
    {
        var currencies = await db.Currencies
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync(ct);

        return currencies.Select(ToResponse).ToList();
    }

    public async Task<CurrencyResponse> GetAsync(string code, CancellationToken ct = default)
    {
        var currency = await db.Currencies.FindAsync([code.ToUpperInvariant()], ct)
            ?? throw new KeyNotFoundException($"Currency '{code}' not found.");

        return ToResponse(currency);
    }

    // ── Helper ───────────────────────────────────────────────────────────

    internal static CurrencyResponse ToResponse(Currency c) =>
        new(c.Code, c.Name, c.Symbol, c.DecimalPlaces, c.IsActive, c.CreatedAt, c.UpdatedAt);
}
