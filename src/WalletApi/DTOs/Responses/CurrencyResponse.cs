namespace WalletApi.DTOs.Responses;

public record CurrencyResponse(
    string Code,
    string Name,
    string Symbol,
    int DecimalPlaces,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
