using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Requests;

public record SetExchangeRateRequest(
    [Required][StringLength(3, MinimumLength = 3)] string FromCurrency,
    [Required][StringLength(3, MinimumLength = 3)] string ToCurrency,
    [Required][Range(0.000001, double.MaxValue)] decimal Rate
);
