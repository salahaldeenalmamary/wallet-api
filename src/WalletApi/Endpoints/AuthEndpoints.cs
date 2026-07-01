using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Auth;
using WalletApi.Services;
using WalletApi.Helpers;

namespace WalletApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
                       .WithTags("Auth")
                       .WithOpenApi();

        // POST /api/auth/register
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var response = await authService.RegisterAsync(request);
            return ApiResults.Created((string?)null, response, "User registered successfully.");
        })
        .WithSummary("Register a new user account.")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        // POST /api/auth/login
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var response = await authService.LoginAsync(request);
            return ApiResults.Ok(response, "Login successful.");
        })
        .WithSummary("Login with email and password to obtain tokens.")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        // POST /api/auth/refresh
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var response = await authService.RefreshAsync(request.RefreshToken);
            return ApiResults.Ok(response, "Token refreshed successfully.");
        })
        .WithSummary("Exchange a valid refresh token for a new access + refresh token pair.")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        // POST /api/auth/logout  (requires auth)
        group.MapPost("/logout", async (
            HttpContext httpContext,
            [FromBody] RefreshTokenRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            await authService.RevokeAsync(request.RefreshToken, userId);
            return ApiResults.NoContent("Logout successful.");
        })
        .WithSummary("Revoke the current refresh token (logout from this device).")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        // GET /api/auth/me  (requires auth)
        group.MapGet("/me", async (
            HttpContext httpContext,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var user = await authService.GetUserByIdAsync(userId);
            if (user is null)
                return Results.NotFound();

            return ApiResults.Ok(new
            {
                id        = user.Id,
                email     = user.Email,
                firstName = user.FirstName,
                lastName  = user.LastName,
                createdAt = user.CreatedAt,
            }, "User profile retrieved successfully.");
        })
        .WithSummary("Get the currently authenticated user's profile.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
