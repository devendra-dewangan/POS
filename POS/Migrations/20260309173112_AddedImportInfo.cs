using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddedImportInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Products_ProductId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_ProductId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "ImportPurchaseTemp");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ImportPurchaseTemp",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "ImportId",
                table: "ImportPurchaseTemp",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "ImportInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TotalRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportType = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportInfos", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPurchaseTemp_ImportInfos_ImportId",
                table: "ImportPurchaseTemp");

            migrationBuilder.DropTable(
                name: "ImportInfos");

            migrationBuilder.DropIndex(
                name: "IX_ImportPurchaseTemp_ImportId",
                table: "ImportPurchaseTemp");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Sales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ImportPurchaseTemp",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportId",
                table: "ImportPurchaseTemp",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ImportPurchaseTemp",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ImportPurchaseTemp",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ImportPurchaseTemp",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "ImportPurchaseTemp",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_ProductId",
                table: "Sales",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Products_ProductId",
                table: "Sales",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
