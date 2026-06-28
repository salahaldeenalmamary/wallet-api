using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Requests;

public record CreateWalletRequest(

    [Required] string Name,
    string? Slug = null,
    string? Description = null,
    string Currency = "USD",
    Dictionary<string, object>? Meta = null
);
