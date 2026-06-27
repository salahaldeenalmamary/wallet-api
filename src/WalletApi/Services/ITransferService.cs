using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;

namespace WalletApi.Services;

public interface ITransferService
{
    Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken ct = default);
    Task<TransferResponse> ForceTransferAsync(TransferRequest request, CancellationToken ct = default);
    Task<TransferResponse?> SafeTransferAsync(TransferRequest request, CancellationToken ct = default);
    Task<TransferResponse> GetByUuidAsync(Guid uuid, CancellationToken ct = default);
}
