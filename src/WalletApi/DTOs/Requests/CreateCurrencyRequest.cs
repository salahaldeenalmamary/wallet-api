using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Requests;

public record CreateCurrencyRequest(
    [Required][StringLength(3, MinimumLength = 3)] string Code,
    [Required] string Name,
    [Required] string Symbol,
    int DecimalPlaces = 2
);
