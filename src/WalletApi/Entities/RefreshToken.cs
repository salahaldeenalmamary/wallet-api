namespace WalletApi.Entities;

/// <summary>
/// Refresh token stored in the database (hashed). Supports token rotation —
/// when a refresh token is consumed, it is revoked and a new one is issued.
/// </summary>
public class RefreshToken
{
    public long Id { get; set; }

    /// <summary>FK → AppUser</summary>
    public long UserId { get; set; }

    /// <summary>SHA-256 hash of the raw token sent to the client.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt  { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    /// <summary>When rotated, holds the hash of the replacement token.</summary>
    public string? ReplacedByTokenHash { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;

    // Computed helpers
    public bool IsExpired  => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked  => RevokedAt is not null;
    public bool IsActive   => !IsRevoked && !IsExpired;
}
