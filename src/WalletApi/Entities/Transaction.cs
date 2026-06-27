using System.Text.Json;
using WalletApi.Domain.Enums;

namespace WalletApi.Entities;

public class Transaction
{
    public long Id { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public long WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;

    /// <summary>Polymorphic owner type, e.g. "User"</summary>
    public string PayableType { get; set; } = string.Empty;
    public long PayableId { get; set; }

    public TransactionType Type { get; set; }

    /// <summary>
    /// Integer-shifted amount (scaled by wallet's DecimalPlaces).
    /// Deposits are positive, withdrawals are stored negative.
    /// </summary>
    public decimal Amount { get; set; }

    public bool Confirmed { get; set; } = true;

    /// <summary>Stored as PostgreSQL jsonb</summary>
    public JsonDocument? Meta { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Transfer? DepositTransfer { get; set; }
    public Transfer? WithdrawTransfer { get; set; }
}
