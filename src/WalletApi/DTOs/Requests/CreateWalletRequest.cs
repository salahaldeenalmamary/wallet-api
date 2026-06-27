using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Requests;

public record CreateWalletRequest(
    [Required] string HolderType,
    [Required] long HolderId,
    [Required] string Name,
    string? Slug = null,
    string? Description = null,
    int DecimalPlaces = 2,
    Dictionary<string, object>? Meta = null
);
