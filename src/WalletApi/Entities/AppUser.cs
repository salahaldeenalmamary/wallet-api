namespace WalletApi.Entities;

/// <summary>Application user — no ASP.NET Core Identity dependency.</summary>
public class AppUser
{
    public long   Id           { get; set; }
    public string Email        { get; set; } = string.Empty;

    /// <summary>PBKDF2-SHA256 hash stored as "iterations.salt.hash" (Base64).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName    { get; set; } = string.Empty;
    public string LastName     { get; set; } = string.Empty;

    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt  { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
