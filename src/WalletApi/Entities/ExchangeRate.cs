namespace WalletApi.Entities;

public class ExchangeRate
{
    public long Id { get; set; }

    /// <summary>Source currency ISO code, e.g. "USD"</summary>
    public string FromCurrency { get; set; } = string.Empty;

    /// <summary>Target currency ISO code, e.g. "EUR"</summary>
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>
    /// How many units of ToCurrency equal 1 unit of FromCurrency.
    /// e.g. Rate=0.92 means 1 USD = 0.92 EUR
    /// </summary>
    public decimal Rate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Currency From { get; set; } = null!;
    public Currency To { get; set; } = null!;
}
