using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;

namespace WalletApi.Services;

public interface IWalletService
{
    Task<WalletResponse> CreateWalletAsync(CreateWalletRequest request, CancellationToken ct = default);
    Task<WalletResponse> GetWalletAsync(long id, CancellationToken ct = default);
    Task<WalletResponse?> GetWalletBySlugAsync(string holderType, long holderId, string slug, CancellationToken ct = default);
    Task<WalletResponse> RefreshBalanceAsync(long walletId, CancellationToken ct = default);
    Task<PagedResponse<TransactionResponse>> GetTransactionsAsync(long walletId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResponse<TransferResponse>> GetTransfersAsync(long walletId, int page, int pageSize, CancellationToken ct = default);
}
