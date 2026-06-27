using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WalletApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    HolderType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    HolderId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Meta = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Balance = table.Column<decimal>(type: "numeric(28,0)", nullable: false, defaultValue: 0m),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<long>(type: "bigint", nullable: false),
                    PayableType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PayableId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(28,0)", nullable: false),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Meta = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    FromId = table.Column<long>(type: "bigint", nullable: false),
                    ToId = table.Column<long>(type: "bigint", nullable: false),
                    DepositId = table.Column<long>(type: "bigint", nullable: false),
                    WithdrawId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Transfer"),
                    StatusLast = table.Column<string>(type: "text", nullable: true),
                    Discount = table.Column<decimal>(type: "numeric(28,0)", nullable: false, defaultValue: 0m),
                    Fee = table.Column<decimal>(type: "numeric(28,0)", nullable: false, defaultValue: 0m),
                    Extra = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Transactions_DepositId",
                        column: x => x.DepositId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Transactions_WithdrawId",
                        column: x => x.WithdrawId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Wallets_FromId",
                        column: x => x.FromId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Wallets_ToId",
                        column: x => x.ToId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PayableType_PayableId",
                table: "Transactions",
                columns: new[] { "PayableType", "PayableId" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PayableType_PayableId_Confirmed",
                table: "Transactions",
                columns: new[] { "PayableType", "PayableId", "Confirmed" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PayableType_PayableId_Type",
                table: "Transactions",
                columns: new[] { "PayableType", "PayableId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PayableType_PayableId_Type_Confirmed",
                table: "Transactions",
                columns: new[] { "PayableType", "PayableId", "Type", "Confirmed" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                table: "Transactions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Uuid",
                table: "Transactions",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DepositId",
                table: "Transfers",
                column: "DepositId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_FromId",
                table: "Transfers",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ToId",
                table: "Transfers",
                column: "ToId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Uuid",
                table: "Transfers",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_WithdrawId",
                table: "Transfers",
                column: "WithdrawId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_HolderType_HolderId_Slug",
                table: "Wallets",
                columns: new[] { "HolderType", "HolderId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Slug",
                table: "Wallets",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Uuid",
                table: "Wallets",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
