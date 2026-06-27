using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;

namespace WalletApi.Services;

public interface ICurrencyService
{
    Task<CurrencyResponse> CreateAsync(CreateCurrencyRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CurrencyResponse>> ListAsync(CancellationToken ct = default);
    Task<CurrencyResponse> GetAsync(string code, CancellationToken ct = default);
}
