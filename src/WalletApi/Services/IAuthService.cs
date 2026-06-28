using WalletApi.DTOs.Auth;
using WalletApi.Entities;

namespace WalletApi.Services;

public interface IAuthService
{
    /// <summary>Register a new user. Returns tokens on success.</summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>Validate credentials and return tokens.</summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Validate a refresh token, revoke it, issue a new pair of tokens (rotation).
    /// </summary>
    Task<AuthResponse> RefreshAsync(string refreshToken);

    /// <summary>Revoke the given refresh token (logout).</summary>
    Task RevokeAsync(string refreshToken, long userId);

    /// <summary>Get the authenticated user's profile.</summary>
    Task<AppUser?> GetUserByIdAsync(long userId);
}
