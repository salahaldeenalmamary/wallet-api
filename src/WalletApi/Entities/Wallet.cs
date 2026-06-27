using System.Text.Json;

namespace WalletApi.Entities;

public class Wallet
{
    public long Id { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();

    /// <summary>Polymorphic owner type, e.g. "User"</summary>
    public string HolderType { get; set; } = string.Empty;

    /// <summary>Polymorphic owner id</summary>
    public long HolderId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly identifier for this wallet within a holder, e.g. "default"</summary>
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Stored as PostgreSQL jsonb</summary>
    public JsonDocument? Meta { get; set; }

    /// <summary>
    /// Balance stored as an integer shifted by DecimalPlaces.
    /// e.g. DecimalPlaces=2, real amount $10.50 → Balance=1050
    /// </summary>
    public decimal Balance { get; set; } = 0;

    /// <summary>Number of decimal places. Derived from the linked Currency.</summary>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>ISO 4217 currency code, e.g. "USD". FK → currencies.code</summary>
    public string Currency { get; set; } = "USD";

    // Navigation
    public Currency CurrencyInfo { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<Transfer> SentTransfers { get; set; } = [];
    public ICollection<Transfer> ReceivedTransfers { get; set; } = [];
}
