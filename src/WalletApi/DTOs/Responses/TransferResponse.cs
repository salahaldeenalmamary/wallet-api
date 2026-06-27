namespace WalletApi.DTOs.Responses;

public record TransferResponse(
    long Id,
    Guid Uuid,
    long FromId,
    long ToId,
    long DepositId,
    long WithdrawId,
    string Status,
    decimal Discount,
    decimal Fee,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
