namespace WalletApi.Entities;

public class Currency
{
    /// <summary>ISO 4217 code, e.g. "USD". Used as primary key.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Currency symbol, e.g. "$"</summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>Default number of decimal places for this currency (e.g. 2 for USD, 8 for BTC)</summary>
    public int DecimalPlaces { get; set; } = 2;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Wallet> Wallets { get; set; } = [];
    public ICollection<ExchangeRate> FromRates { get; set; } = [];
    public ICollection<ExchangeRate> ToRates { get; set; } = [];
}
