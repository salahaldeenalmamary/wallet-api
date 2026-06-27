using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WalletApi.Data;
using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;
using WalletApi.Entities;

namespace WalletApi.Services;

public class WalletService(WalletDbContext db) : IWalletService
{
    public async Task<WalletResponse> CreateWalletAsync(CreateWalletRequest request, CancellationToken ct = default)
    {
        var currencyCode = request.Currency.ToUpperInvariant();

        // Auto-derive DecimalPlaces from the currency definition
        var currency = await db.Currencies.FindAsync([currencyCode], ct)
            ?? throw new KeyNotFoundException($"Currency '{currencyCode}' not found. Create it first via POST /currencies.");

        if (!currency.IsActive)
            throw new InvalidOperationException($"Currency '{currencyCode}' is inactive.");

        var slug = request.Slug ?? SlugFrom(request.Name);

        var wallet = new Wallet
        {
            HolderType = request.HolderType,
            HolderId = request.HolderId,
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Currency = currencyCode,
            DecimalPlaces = currency.DecimalPlaces,
            Balance = 0,
            Meta = request.Meta is not null
                ? JsonDocument.Parse(JsonSerializer.Serialize(request.Meta))
                : null
        };

        db.Wallets.Add(wallet);
        await db.SaveChangesAsync(ct);

        return ToResponse(wallet);
    }

    public async Task<WalletResponse> GetWalletAsync(long id, CancellationToken ct = default)
    {
        var wallet = await db.Wallets.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Wallet {id} not found.");

        return ToResponse(wallet);
    }

    public async Task<WalletResponse?> GetWalletBySlugAsync(
        string holderType, long holderId, string slug, CancellationToken ct = default)
    {
        var wallet = await db.Wallets
            .FirstOrDefaultAsync(w => w.HolderType == holderType
                                   && w.HolderId == holderId
                                   && w.Slug == slug, ct);

        return wallet is null ? null : ToResponse(wallet);
    }

    public async Task<WalletResponse> RefreshBalanceAsync(long walletId, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, ct);

        var wallet = await db.Wallets
            .FromSqlRaw("SELECT * FROM wallets WHERE id = {0} FOR UPDATE", walletId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");

        var computedBalance = await db.Transactions
            .Where(t => t.WalletId == walletId && t.Confirmed)
            .SumAsync(t => t.Amount, ct);

        wallet.Balance = computedBalance;
        wallet.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return ToResponse(wallet);
    }

    public async Task<PagedResponse<TransactionResponse>> GetTransactionsAsync(
        long walletId, int page, int pageSize, CancellationToken ct = default)
    {
        _ = await db.Wallets.FindAsync([walletId], ct)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");

        var query = db.Transactions.Where(t => t.WalletId == walletId);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var wallet = (await db.Wallets.FindAsync([walletId], ct))!;
        return new PagedResponse<TransactionResponse>(
            items.Select(t => ToTransactionResponse(t, wallet.DecimalPlaces)),
            page, pageSize, total);
    }

    public async Task<PagedResponse<TransferResponse>> GetTransfersAsync(
        long walletId, int page, int pageSize, CancellationToken ct = default)
    {
        _ = await db.Wallets.FindAsync([walletId], ct)
            ?? throw new KeyNotFoundException($"Wallet {walletId} not found.");

        var query = db.Transfers.Where(t => t.FromId == walletId || t.ToId == walletId);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<TransferResponse>(
            items.Select(ToTransferResponse), page, pageSize, total);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    internal static WalletResponse ToResponse(Wallet w) => new(
        w.Id, w.Uuid, w.HolderType, w.HolderId,
        w.Name, w.Slug, w.Description,
        w.Currency,
        w.Balance,
        BalanceFloat(w.Balance, w.DecimalPlaces),
        w.DecimalPlaces,
        w.CreatedAt, w.UpdatedAt);

    internal static TransactionResponse ToTransactionResponse(Transaction t, int decimalPlaces) => new(
        t.Id, t.Uuid, t.WalletId, t.PayableType, t.PayableId,
        t.Type.ToString(),
        t.Amount,
        BalanceFloat(t.Amount, decimalPlaces),
        t.Confirmed, t.CreatedAt, t.UpdatedAt);

    internal static TransferResponse ToTransferResponse(Transfer t) => new(
        t.Id, t.Uuid, t.FromId, t.ToId, t.DepositId, t.WithdrawId,
        t.Status.ToString(), t.Discount, t.Fee, t.CreatedAt, t.UpdatedAt);

    private static decimal BalanceFloat(decimal amount, int decimalPlaces) =>
        amount / (decimal)Math.Pow(10, decimalPlaces);

    private static string SlugFrom(string name) =>
        System.Text.RegularExpressions.Regex.Replace(name.ToLower().Trim(), @"[^a-z0-9]+", "-").Trim('-');
}
