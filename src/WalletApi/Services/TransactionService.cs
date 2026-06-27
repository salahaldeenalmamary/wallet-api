using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.Domain.Enums;
using WalletApi.Domain.Exceptions;
using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;
using WalletApi.Entities;

namespace WalletApi.Services;

public class TransactionService(WalletDbContext db) : ITransactionService
{
    public async Task<TransactionResponse> DepositAsync(
        long walletId, DepositRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new AmountInvalidException();

        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        var wallet = await LockWalletAsync(walletId, ct);
        var scaledAmount = Scale(request.Amount, wallet.DecimalPlaces);

        var transaction = new Transaction
        {
            WalletId = walletId,
            PayableType = wallet.HolderType,
            PayableId = wallet.HolderId,
            Type = TransactionType.Deposit,
            Amount = scaledAmount,
            Confirmed = request.Confirmed,
            Meta = request.Meta is not null
                ? JsonDocument.Parse(JsonSerializer.Serialize(request.Meta))
                : null
        };

        db.Transactions.Add(transaction);

        if (request.Confirmed)
        {
            wallet.Balance += scaledAmount;
            wallet.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return WalletService.ToTransactionResponse(transaction, wallet.DecimalPlaces);
    }

    public async Task<TransactionResponse> WithdrawAsync(
        long walletId, WithdrawRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new AmountInvalidException();

        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        var wallet = await LockWalletAsync(walletId, ct);
        var scaledAmount = Scale(request.Amount, wallet.DecimalPlaces);

        if (wallet.Balance == 0)
            throw new BalanceIsEmptyException();

        if (wallet.Balance < scaledAmount)
            throw new InsufficientFundsException();

        var transaction = CreateWithdrawTransaction(wallet, scaledAmount, request.Confirmed, request.Meta);
        db.Transactions.Add(transaction);

        if (request.Confirmed)
        {
            wallet.Balance -= scaledAmount;
            wallet.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return WalletService.ToTransactionResponse(transaction, wallet.DecimalPlaces);
    }

    public async Task<TransactionResponse> ForceWithdrawAsync(
        long walletId, WithdrawRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new AmountInvalidException();

        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        var wallet = await LockWalletAsync(walletId, ct);
        var scaledAmount = Scale(request.Amount, wallet.DecimalPlaces);

        var transaction = CreateWithdrawTransaction(wallet, scaledAmount, request.Confirmed, request.Meta);
        db.Transactions.Add(transaction);

        if (request.Confirmed)
        {
            wallet.Balance -= scaledAmount;
            wallet.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return WalletService.ToTransactionResponse(transaction, wallet.DecimalPlaces);
    }

    public async Task<bool> CanWithdrawAsync(
        long walletId, decimal amount, bool allowZero = false, CancellationToken ct = default)
    {
        var wallet = await db.Wallets.FindAsync([walletId], ct)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");

        var scaledAmount = Scale(amount, wallet.DecimalPlaces);
        return allowZero
            ? wallet.Balance >= scaledAmount
            : wallet.Balance > 0 && wallet.Balance >= scaledAmount;
    }

    public async Task<TransactionResponse> ConfirmAsync(Guid uuid, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        var transaction = await db.Transactions
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.Uuid == uuid, ct)
            ?? throw new KeyNotFoundException($"Transaction {uuid} not found.");

        if (transaction.Confirmed)
            return WalletService.ToTransactionResponse(transaction, transaction.Wallet.DecimalPlaces);

        transaction.Confirmed = true;
        transaction.UpdatedAt = DateTime.UtcNow;

        // Apply balance change
        if (transaction.Type == TransactionType.Deposit)
            transaction.Wallet.Balance += transaction.Amount;
        else
            transaction.Wallet.Balance -= Math.Abs(transaction.Amount);

        transaction.Wallet.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return WalletService.ToTransactionResponse(transaction, transaction.Wallet.DecimalPlaces);
    }

    public async Task<TransactionResponse> RevertAsync(Guid uuid, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        var transaction = await db.Transactions
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.Uuid == uuid, ct)
            ?? throw new KeyNotFoundException($"Transaction {uuid} not found.");

        if (!transaction.Confirmed)
            return WalletService.ToTransactionResponse(transaction, transaction.Wallet.DecimalPlaces);

        transaction.Confirmed = false;
        transaction.UpdatedAt = DateTime.UtcNow;

        // Reverse balance change
        if (transaction.Type == TransactionType.Deposit)
            transaction.Wallet.Balance -= transaction.Amount;
        else
            transaction.Wallet.Balance += Math.Abs(transaction.Amount);

        transaction.Wallet.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return WalletService.ToTransactionResponse(transaction, transaction.Wallet.DecimalPlaces);
    }

    public async Task<TransactionResponse> GetByUuidAsync(Guid uuid, CancellationToken ct = default)
    {
        var transaction = await db.Transactions
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.Uuid == uuid, ct)
            ?? throw new KeyNotFoundException($"Transaction {uuid} not found.");

        return WalletService.ToTransactionResponse(transaction, transaction.Wallet.DecimalPlaces);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    /// <summary>Locks the wallet row for update within a serializable transaction.</summary>
    private async Task<Wallet> LockWalletAsync(long walletId, CancellationToken ct)
    {
        return await db.Wallets
            .FromSqlRaw("SELECT * FROM wallets WHERE id = {0} AND deleted_at IS NULL FOR UPDATE", walletId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");
    }

    private static Transaction CreateWithdrawTransaction(
        Wallet wallet, decimal scaledAmount, bool confirmed, Dictionary<string, object>? meta) =>
        new()
        {
            WalletId = wallet.Id,
            PayableType = wallet.HolderType,
            PayableId = wallet.HolderId,
            Type = TransactionType.Withdraw,
            // Withdrawals stored as negative amounts (mirrors Laravel behaviour)
            Amount = -scaledAmount,
            Confirmed = confirmed,
            Meta = meta is not null
                ? JsonDocument.Parse(JsonSerializer.Serialize(meta))
                : null
        };

    private static decimal Scale(decimal amount, int decimalPlaces) =>
        Math.Round(amount * (decimal)Math.Pow(10, decimalPlaces));
}
