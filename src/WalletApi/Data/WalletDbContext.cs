using Microsoft.EntityFrameworkCore;
using WalletApi.Domain.Enums;
using WalletApi.Entities;

namespace WalletApi.Data;

public class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Currency ──────────────────────────────────────────────────────
        modelBuilder.Entity<Currency>(e =>
        {
            e.HasKey(c => c.Code);
            e.Property(c => c.Code).HasMaxLength(3).IsRequired();
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Symbol).HasMaxLength(10).IsRequired();
            e.Property(c => c.DecimalPlaces).HasDefaultValue(2);
            e.Property(c => c.IsActive).HasDefaultValue(true);
            e.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");

            // Seed common currencies
            e.HasData(
                new Currency { Code = "USD", Name = "US Dollar",          Symbol = "$",  DecimalPlaces = 2 },
                new Currency { Code = "EUR", Name = "Euro",               Symbol = "€",  DecimalPlaces = 2 },
                new Currency { Code = "GBP", Name = "British Pound",      Symbol = "£",  DecimalPlaces = 2 },
                new Currency { Code = "SAR", Name = "Saudi Riyal",        Symbol = "﷼",  DecimalPlaces = 2 },
                new Currency { Code = "AED", Name = "UAE Dirham",         Symbol = "د.إ", DecimalPlaces = 2 },
                new Currency { Code = "JPY", Name = "Japanese Yen",       Symbol = "¥",  DecimalPlaces = 0 },
                new Currency { Code = "BTC", Name = "Bitcoin",            Symbol = "₿",  DecimalPlaces = 8 }
            );
        });

        // ── ExchangeRate ───────────────────────────────────────────────────
        modelBuilder.Entity<ExchangeRate>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.FromCurrency).HasMaxLength(3).IsRequired();
            e.Property(r => r.ToCurrency).HasMaxLength(3).IsRequired();
            e.Property(r => r.Rate).HasColumnType("numeric(28,10)").IsRequired();
            e.Property(r => r.CreatedAt).HasDefaultValueSql("NOW()");

            e.HasIndex(r => new { r.FromCurrency, r.ToCurrency });
            e.HasIndex(r => new { r.FromCurrency, r.ToCurrency, r.CreatedAt });

            e.HasOne(r => r.From)
             .WithMany(c => c.FromRates)
             .HasForeignKey(r => r.FromCurrency)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.To)
             .WithMany(c => c.ToRates)
             .HasForeignKey(r => r.ToCurrency)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Wallet ───────────────────────────────────────────────────────
        modelBuilder.Entity<Wallet>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.Uuid).IsRequired();
            e.HasIndex(w => w.Uuid).IsUnique();

            e.Property(w => w.HolderType).IsRequired().HasMaxLength(255);
            e.Property(w => w.HolderId).IsRequired();
            e.Property(w => w.Name).IsRequired().HasMaxLength(255);
            e.Property(w => w.Slug).IsRequired().HasMaxLength(255);
            e.HasIndex(w => w.Slug);
            // Unique: one slug per holder
            e.HasIndex(w => new { w.HolderType, w.HolderId, w.Slug }).IsUnique();

            e.Property(w => w.Description).HasMaxLength(1024);
            e.Property(w => w.Meta).HasColumnType("jsonb");
            e.Property(w => w.Balance).HasColumnType("numeric(28,0)").HasDefaultValue(0m);
            e.Property(w => w.DecimalPlaces).HasDefaultValue(2);
            e.Property(w => w.Currency).HasMaxLength(3).HasDefaultValue("USD").IsRequired();

            e.Property(w => w.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(w => w.UpdatedAt).HasDefaultValueSql("NOW()");

            // Soft delete filter
            e.HasQueryFilter(w => w.DeletedAt == null);

            // FK → currencies
            e.HasOne(w => w.CurrencyInfo)
             .WithMany(c => c.Wallets)
             .HasForeignKey(w => w.Currency)
             .OnDelete(DeleteBehavior.Restrict);

            // Relations
            e.HasMany(w => w.Transactions)
             .WithOne(t => t.Wallet)
             .HasForeignKey(t => t.WalletId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(w => w.SentTransfers)
             .WithOne(t => t.From)
             .HasForeignKey(t => t.FromId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(w => w.ReceivedTransfers)
             .WithOne(t => t.To)
             .HasForeignKey(t => t.ToId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Transaction ───────────────────────────────────────────────────
        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Uuid).IsRequired();
            e.HasIndex(t => t.Uuid).IsUnique();

            e.Property(t => t.PayableType).IsRequired().HasMaxLength(255);
            e.Property(t => t.PayableId).IsRequired();

            e.Property(t => t.Type)
             .HasConversion<string>()
             .IsRequired();

            e.HasIndex(t => t.Type);
            e.HasIndex(t => new { t.PayableType, t.PayableId });
            e.HasIndex(t => new { t.PayableType, t.PayableId, t.Type });
            e.HasIndex(t => new { t.PayableType, t.PayableId, t.Confirmed });
            e.HasIndex(t => new { t.PayableType, t.PayableId, t.Type, t.Confirmed });

            e.Property(t => t.Amount).HasColumnType("numeric(28,0)").IsRequired();
            e.Property(t => t.Confirmed).IsRequired();
            e.Property(t => t.Meta).HasColumnType("jsonb");

            e.Property(t => t.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(t => t.UpdatedAt).HasDefaultValueSql("NOW()");

            // Soft delete filter
            e.HasQueryFilter(t => t.DeletedAt == null);
        });

        // ── Transfer ──────────────────────────────────────────────────────
        modelBuilder.Entity<Transfer>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Uuid).IsRequired();
            e.HasIndex(t => t.Uuid).IsUnique();

            e.Property(t => t.Status)
             .HasConversion<string>()
             .HasDefaultValue(TransferStatus.Transfer)
             .IsRequired();

            e.Property(t => t.StatusLast).HasConversion<string>();

            e.Property(t => t.Discount).HasColumnType("numeric(28,0)").HasDefaultValue(0m);
            e.Property(t => t.Fee).HasColumnType("numeric(28,0)").HasDefaultValue(0m);
            e.Property(t => t.Extra).HasColumnType("jsonb");

            e.Property(t => t.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(t => t.UpdatedAt).HasDefaultValueSql("NOW()");

            // Soft delete filter
            e.HasQueryFilter(t => t.DeletedAt == null);

            e.HasOne(t => t.Deposit)
             .WithOne(tx => tx.DepositTransfer)
             .HasForeignKey<Transfer>(t => t.DepositId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(t => t.Withdraw)
             .WithOne(tx => tx.WithdrawTransfer)
             .HasForeignKey<Transfer>(t => t.WithdrawId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
