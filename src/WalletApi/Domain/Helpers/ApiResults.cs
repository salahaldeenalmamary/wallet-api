using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WalletApi.Helpers;

public static class ApiResults
{
    public static IResult Ok(object? data = null, string? message = "Success.")
    {
        return Results.Ok(new { success = true, message, data });
    }

    public static IResult Created(string? uri, object? data = null, string? message = "Resource created successfully.")
    {
        return Results.Created(uri ?? string.Empty, new { success = true, message, data });
    }

    public static IResult NoContent(string? message = "Operation successful.")
    {
        return Results.Ok(new { success = true, message, data = (object?)null });
    }

    public static IResult BadRequest(string? message = "Bad request.")
    {
        return Results.BadRequest(new { success = false, message, data = (object?)null });
    }
    
    public static IResult NotFound(string? message = "Resource not found.")
    {
        return Results.NotFound(new { success = false, message, data = (object?)null });
    }
}
