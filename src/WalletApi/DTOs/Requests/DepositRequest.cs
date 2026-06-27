using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Requests;

public record DepositRequest(
    [Required][Range(0.01, double.MaxValue)] decimal Amount,
    bool Confirmed = true,
    Dictionary<string, object>? Meta = null
);
