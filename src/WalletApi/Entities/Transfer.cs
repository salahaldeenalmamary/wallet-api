using System.Text.Json;
using WalletApi.Domain.Enums;

namespace WalletApi.Entities;

public class Transfer
{
    public long Id { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public long FromId { get; set; }
    public Wallet From { get; set; } = null!;

    public long ToId { get; set; }
    public Wallet To { get; set; } = null!;

    public long DepositId { get; set; }
    public Transaction Deposit { get; set; } = null!;

    public long WithdrawId { get; set; }
    public Transaction Withdraw { get; set; } = null!;

    public TransferStatus Status { get; set; } = TransferStatus.Transfer;
    public TransferStatus? StatusLast { get; set; }

    /// <summary>Integer-shifted discount amount</summary>
    public decimal Discount { get; set; } = 0;

    /// <summary>Integer-shifted fee amount</summary>
    public decimal Fee { get; set; } = 0;

    /// <summary>Stored as PostgreSQL jsonb</summary>
    public JsonDocument? Extra { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
