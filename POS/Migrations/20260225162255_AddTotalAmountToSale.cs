using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalAmountToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Products_ProductId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Products_ProductId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_ProductId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "SaleItems",
                newName: "BatchId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems",
                newName: "IX_SaleItems_BatchId");

            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "Sales",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseStock",
                table: "Batches",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Buyers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BuyerId",
                table: "Sales",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Batches_BatchId",
                table: "SaleItems",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Buyers_BuyerId",
                table: "Sales",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Batches_BatchId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Buyers_BuyerId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "Buyers");

            migrationBuilder.DropIndex(
                name: "IX_Sales_BuyerId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PurchaseStock",
                table: "Batches");

            migrationBuilder.RenameColumn(
                name: "BatchId",
                table: "SaleItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_BatchId",
                table: "SaleItems",
                newName: "IX_SaleItems_ProductId");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Purchases",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Stock",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ProductId",
                table: "Purchases",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Products_ProductId",
                table: "Purchases",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Products_ProductId",
                table: "SaleItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
