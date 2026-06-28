using System.ComponentModel.DataAnnotations;

namespace WalletApi.DTOs.Auth;

/// <summary>Request body for user login.</summary>
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
