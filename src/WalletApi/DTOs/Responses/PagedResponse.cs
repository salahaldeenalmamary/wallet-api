namespace WalletApi.DTOs.Responses;

public record PagedResponse<T>(
    IEnumerable<T> Data,
    int Page,
    int PageSize,
    int Total
);
