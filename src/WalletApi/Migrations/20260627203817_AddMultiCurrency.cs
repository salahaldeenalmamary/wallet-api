using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WalletApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Wallets",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ToCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(28,10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies_FromCurrency",
                        column: x => x.FromCurrency,
                        principalTable: "Currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies_ToCurrency",
                        column: x => x.ToCurrency,
                        principalTable: "Currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Code", "CreatedAt", "DecimalPlaces", "IsActive", "Name", "Symbol", "UpdatedAt" },
                values: new object[,]
                {
                    { "AED", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), 2, true, "UAE Dirham", "د.إ", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) },
                    { "BTC", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), 8, true, "Bitcoin", "₿", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4420) },
                    { "EUR", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4400), 2, true, "Euro", "€", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) },
                    { "GBP", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), 2, true, "British Pound", "£", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) }
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Code", "CreatedAt", "IsActive", "Name", "Symbol", "UpdatedAt" },
                values: new object[] { "JPY", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), true, "Japanese Yen", "¥", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Code", "CreatedAt", "DecimalPlaces", "IsActive", "Name", "Symbol", "UpdatedAt" },
                values: new object[,]
                {
                    { "SAR", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), 2, true, "Saudi Riyal", "﷼", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) },
                    { "USD", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(3560), 2, true, "US Dollar", "$", new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(3560) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Currency",
                table: "Wallets",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_FromCurrency_ToCurrency",
                table: "ExchangeRates",
                columns: new[] { "FromCurrency", "ToCurrency" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_FromCurrency_ToCurrency_CreatedAt",
                table: "ExchangeRates",
                columns: new[] { "FromCurrency", "ToCurrency", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_ToCurrency",
                table: "ExchangeRates",
                column: "ToCurrency");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Currencies_Currency",
                table: "Wallets",
                column: "Currency",
                principalTable: "Currencies",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Currencies_Currency",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_Currency",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Wallets");
        }
    }
}
