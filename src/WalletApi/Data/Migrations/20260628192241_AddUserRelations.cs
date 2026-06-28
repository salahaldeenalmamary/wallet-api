using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Wallets_HolderId",
                table: "Wallets",
                column: "HolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PayableId",
                table: "Transactions",
                column: "PayableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_PayableId",
                table: "Transactions",
                column: "PayableId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Users_HolderId",
                table: "Wallets",
                column: "HolderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_PayableId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_HolderId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_HolderId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PayableId",
                table: "Transactions");
        }
    }
}
