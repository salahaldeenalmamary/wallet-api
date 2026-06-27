using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Requests;

public record TransferRequest(
    [Required] long FromWalletId,
    [Required] long ToWalletId,
    [Required][Range(0.01, double.MaxValue)] decimal Amount,
    Dictionary<string, object>? Meta = null
);
