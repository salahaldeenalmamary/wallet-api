using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;

namespace WalletApi.Services;

public interface IExchangeRateService
{
    /// <summary>Creates or replaces the rate for a given currency pair.</summary>
    Task<ExchangeRateResponse> SetRateAsync(SetExchangeRateRequest request, CancellationToken ct = default);

    /// <summary>Returns the most recent rate for the given pair. Throws if not found.</summary>
    Task<ExchangeRateResponse> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken ct = default);

    /// <summary>Lists all exchange rates.</summary>
    Task<IReadOnlyList<ExchangeRateResponse>> ListAsync(CancellationToken ct = default);

    /// <summary>
    /// Converts an amount from one currency to another using the stored rate.
    /// Returns the same amount if both currencies are equal.
    /// </summary>
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken ct = default);
}
