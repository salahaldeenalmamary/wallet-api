using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WalletApi.Data;
using WalletApi.DTOs.Auth;
using WalletApi.Entities;

namespace WalletApi.Services;

public class AuthService(WalletDbContext db, IConfiguration configuration) : IAuthService
{
    private readonly WalletDbContext _db = db;
    private readonly IConfiguration _configuration = configuration;

    // ── Constants ────────────────────────────────────────────────────────────────
    private const int Pbkdf2Iterations = 350_000;
    private const int SaltSize = 32; // bytes
    private const int HashSize = 32; // bytes

    // ── Public API ───────────────────────────────────────────────────────────────

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (exists)
            throw new InvalidOperationException("Email is already registered.");

        var user = new AppUser
        {
            Email = normalizedEmail,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Wallets =
            [
                new Wallet
                {
                    HolderType = "AppUser",
                    Name = "Default",
                    Slug = "default",
                    Currency = "USD",
                    DecimalPlaces = 2
                }
            ]
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // Rotate: revoke old token
        stored.RevokedAt = DateTime.UtcNow;

        // Issue new token pair
        var newRaw = GenerateRawRefreshToken();
        var newHash = HashToken(newRaw);
        stored.ReplacedByTokenHash = newHash;

        var newRefreshToken = new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
        };
        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        var accessToken = GenerateAccessToken(stored.User);
        var expiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes());

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRaw,
            ExpiresAt = expiresAt,
            UserId = stored.User.Id,
            Email = stored.User.Email,
            FirstName = stored.User.FirstName,
            LastName = stored.User.LastName,
        };
    }

    public async Task RevokeAsync(string refreshToken, long userId)
    {
        var tokenHash = HashToken(refreshToken);

        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId);

        if (stored is null || !stored.IsActive)
            throw new InvalidOperationException("Refresh token not found or already revoked.");

        stored.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<AppUser?> GetUserByIdAsync(long userId)
        => await _db.Users.FindAsync(userId);

    // ── Private Helpers ──────────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(AppUser user)
    {
        var rawRefreshToken = GenerateRawRefreshToken();
        var tokenHash = HashToken(rawRefreshToken);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
        };
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes());

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }

    private string GenerateAccessToken(AppUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
            }),
            Expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }

    // ── Password Hashing ─────────────────────────────────────────────────────────

    private static string HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: Pbkdf2Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: HashSize);

        return $"{Pbkdf2Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out var iterations)) return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: HashSize);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    // ── Refresh Token Helpers ────────────────────────────────────────────────────

    private static string GenerateRawRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private int GetAccessTokenExpiryMinutes()
        => int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var v) ? v : 15;

    private int GetRefreshTokenExpiryDays()
        => int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var v) ? v : 7;
}