namespace WalletApi.DTOs.Responses;

public record WalletResponse(
    long Id,
    Guid Uuid,
    string HolderType,
    long HolderId,
    string Name,
    string Slug,
    string? Description,
    string Currency,
    decimal Balance,
    decimal BalanceFloat,
    int DecimalPlaces,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
