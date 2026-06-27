using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;

namespace WalletApi.Services;

public interface ITransactionService
{
    Task<TransactionResponse> DepositAsync(long walletId, DepositRequest request, CancellationToken ct = default);
    Task<TransactionResponse> WithdrawAsync(long walletId, WithdrawRequest request, CancellationToken ct = default);
    Task<TransactionResponse> ForceWithdrawAsync(long walletId, WithdrawRequest request, CancellationToken ct = default);
    Task<bool> CanWithdrawAsync(long walletId, decimal amount, bool allowZero = false, CancellationToken ct = default);
    Task<TransactionResponse> ConfirmAsync(Guid uuid, CancellationToken ct = default);
    Task<TransactionResponse> RevertAsync(Guid uuid, CancellationToken ct = default);
    Task<TransactionResponse> GetByUuidAsync(Guid uuid, CancellationToken ct = default);
}
