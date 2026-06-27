namespace WalletApi.DTOs.Responses;

public record ExchangeRateResponse(
    long Id,
    string FromCurrency,
    string ToCurrency,
    decimal Rate,
    DateTime CreatedAt
);
