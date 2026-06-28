using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WalletApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndFixSeedDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "AED",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "BTC",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "EUR",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "GBP",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "JPY",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "SAR",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "USD",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "AED",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "BTC",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4420) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "EUR",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4400), new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "GBP",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "JPY",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "SAR",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410), new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(4410) });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Code",
                keyValue: "USD",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(3560), new DateTime(2026, 6, 27, 20, 38, 17, 405, DateTimeKind.Utc).AddTicks(3560) });
        }
    }
}
