using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.Domain.Enums;
using WalletApi.Domain.Exceptions;
using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;
using WalletApi.Entities;

namespace WalletApi.Services;

public class TransferService(WalletDbContext db, IExchangeRateService exchangeRateService) : ITransferService
{
    public async Task<TransferResponse> TransferAsync(
        TransferRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new AmountInvalidException();

        return await ExecuteTransferAsync(request, force: false, ct);
    }

    public async Task<TransferResponse> ForceTransferAsync(
        TransferRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new AmountInvalidException();

        return await ExecuteTransferAsync(request, force: true, ct);
    }

    public async Task<TransferResponse?> SafeTransferAsync(
        TransferRequest request, CancellationToken ct = default)
    {
        try
        {
            return await TransferAsync(request, ct);
        }
        catch (Exception ex) when (ex is BalanceIsEmptyException or InsufficientFundsException)
        {
            return null;
        }
    }

    public async Task<TransferResponse> GetByUuidAsync(Guid uuid, CancellationToken ct = default)
    {
        var transfer = await db.Transfers
            .FirstOrDefaultAsync(t => t.Uuid == uuid, ct)
            ?? throw new KeyNotFoundException($"Transfer {uuid} not found.");

        return WalletService.ToTransferResponse(transfer);
    }

    // ── Core logic ────────────────────────────────────────────────────────

    private async Task<TransferResponse> ExecuteTransferAsync(
        TransferRequest request, bool force, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        // Lock both wallets (order by ID to prevent deadlocks)
        var (first, second) = request.FromWalletId < request.ToWalletId
            ? (request.FromWalletId, request.ToWalletId)
            : (request.ToWalletId, request.FromWalletId);

        var firstWallet  = await LockWalletAsync(first, ct);
        var secondWallet = await LockWalletAsync(second, ct);

        var fromWallet = firstWallet.Id == request.FromWalletId ? firstWallet : secondWallet;
        var toWallet   = firstWallet.Id == request.ToWalletId   ? firstWallet : secondWallet;

        var scaledWithdraw = Scale(request.Amount, fromWallet.DecimalPlaces);

        if (!force)
        {
            if (fromWallet.Balance == 0)
                throw new BalanceIsEmptyException();
            if (fromWallet.Balance < scaledWithdraw)
                throw new InsufficientFundsException();
        }

        // ── Cross-currency conversion ─────────────────────────────────────
        decimal depositAmount;
        decimal? appliedRate = null;

        if (fromWallet.Currency.Equals(toWallet.Currency, StringComparison.OrdinalIgnoreCase))
        {
            // Same currency — re-scale for the target wallet's decimal places
            depositAmount = Scale(request.Amount, toWallet.DecimalPlaces);
        }
        else
        {
            // Different currencies — apply exchange rate
            var rateResponse = await exchangeRateService.GetRateAsync(
                fromWallet.Currency, toWallet.Currency, ct);

            appliedRate = rateResponse.Rate;

            // Convert the raw (unscaled) amount, then scale to target decimals
            var convertedRaw = request.Amount * rateResponse.Rate;
            depositAmount = Scale(convertedRaw, toWallet.DecimalPlaces);
        }

        // Create withdraw transaction on the source wallet
        var withdrawTx = new Transaction
        {
            WalletId    = fromWallet.Id,
            PayableType = fromWallet.HolderType,
            PayableId   = fromWallet.HolderId,
            Type        = TransactionType.Withdraw,
            Amount      = -scaledWithdraw,
            Confirmed   = true
        };

        // Create deposit transaction on the target wallet
        var depositTx = new Transaction
        {
            WalletId    = toWallet.Id,
            PayableType = toWallet.HolderType,
            PayableId   = toWallet.HolderId,
            Type        = TransactionType.Deposit,
            Amount      = depositAmount,
            Confirmed   = true
        };

        db.Transactions.Add(withdrawTx);
        db.Transactions.Add(depositTx);
        await db.SaveChangesAsync(ct); // Get IDs assigned

        // Update balances
        fromWallet.Balance -= scaledWithdraw;
        fromWallet.UpdatedAt = DateTime.UtcNow;
        toWallet.Balance += depositAmount;
        toWallet.UpdatedAt = DateTime.UtcNow;

        // Build audit JSON for the transfer record
        var extraPayload = new Dictionary<string, object>
        {
            ["from_currency"]  = fromWallet.Currency,
            ["to_currency"]    = toWallet.Currency,
            ["original_amount"] = request.Amount
        };
        if (appliedRate.HasValue)
            extraPayload["exchange_rate"] = appliedRate.Value;

        var transfer = new Transfer
        {
            FromId     = fromWallet.Id,
            ToId       = toWallet.Id,
            WithdrawId = withdrawTx.Id,
            DepositId  = depositTx.Id,
            Status     = TransferStatus.Transfer,
            Extra      = JsonDocument.Parse(JsonSerializer.Serialize(extraPayload))
        };

        db.Transfers.Add(transfer);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return WalletService.ToTransferResponse(transfer);
    }

    private async Task<Wallet> LockWalletAsync(long walletId, CancellationToken ct)
    {
        return await db.Wallets
            .FromSqlRaw("SELECT * FROM wallets WHERE id = {0} AND deleted_at IS NULL FOR UPDATE", walletId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");
    }

    private static decimal Scale(decimal amount, int decimalPlaces) =>
        Math.Round(amount * (decimal)Math.Pow(10, decimalPlaces));
}
