namespace WalletApi.DTOs.Auth;

/// <summary>Returned on successful login or token refresh.</summary>
public class AuthResponse
{
    public string AccessToken  { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt  { get; set; }
    public long   UserId       { get; set; }
    public string Email        { get; set; } = string.Empty;
    public string FirstName    { get; set; } = string.Empty;
    public string LastName     { get; set; } = string.Empty;
}
