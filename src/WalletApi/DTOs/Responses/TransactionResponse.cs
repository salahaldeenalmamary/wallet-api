namespace WalletApi.DTOs.Responses;

public record TransactionResponse(
    long Id,
    Guid Uuid,
    long WalletId,
    string PayableType,
    long PayableId,
    string Type,
    decimal Amount,
    decimal AmountFloat,
    bool Confirmed,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
