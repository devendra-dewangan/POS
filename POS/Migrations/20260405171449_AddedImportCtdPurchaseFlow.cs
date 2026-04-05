using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddedImportCtdPurchaseFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPurchaseTemp_ImportInfos_ImportId",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropIndex(
                name: "IX_ImportPurchaseTemp_ImportId",
                table: "ImportPurchaseTemp");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalStock",
                table: "Products",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "ImportInfoId",
                table: "ImportPurchaseTemp",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "Batches",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportPurchaseTemp_ImportInfoId",
                table: "ImportPurchaseTemp",
                column: "ImportInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPurchaseTemp_ImportInfos_ImportInfoId",
                table: "ImportPurchaseTemp",
                column: "ImportInfoId",
                principalTable: "ImportInfos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPurchaseTemp_ImportInfos_ImportInfoId",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropIndex(
                name: "IX_ImportPurchaseTemp_ImportInfoId",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropColumn(
                name: "ImportInfoId",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "Batches");

            migrationBuilder.AlterColumn<int>(
                name: "TotalStock",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPurchaseTemp_ImportId",
                table: "ImportPurchaseTemp",
                column: "ImportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPurchaseTemp_ImportInfos_ImportId",
                table: "ImportPurchaseTemp",
                column: "ImportId",
                principalTable: "ImportInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
