using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Auth;

/// <summary>Request body for refreshing an access token.</summary>
public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
