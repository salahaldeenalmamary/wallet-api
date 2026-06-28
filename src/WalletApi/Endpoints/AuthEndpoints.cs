using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WalletApi.DTOs.Auth;
using WalletApi.Services;

namespace WalletApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
                       .WithTags("Auth")
                       .WithOpenApi();

        // POST /api/auth/register
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IAuthService authService,
            CancellationToken ct) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request);
                return Results.Created((string?)null, response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
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
            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
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
            try
            {
                var response = await authService.RefreshAsync(request.RefreshToken);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
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

            try
            {
                await authService.RevokeAsync(request.RefreshToken, userId);
                return Results.NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
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

            return Results.Ok(new
            {
                id        = user.Id,
                email     = user.Email,
                firstName = user.FirstName,
                lastName  = user.LastName,
                createdAt = user.CreatedAt,
            });
        })
        .WithSummary("Get the currently authenticated user's profile.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();
    }
}
