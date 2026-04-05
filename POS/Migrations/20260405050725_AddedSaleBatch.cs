using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddedSaleBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_Purchases_PurchaseId",
                table: "Batches");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Batches_BatchId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_BatchId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "PurchaseRate",
                table: "Batches");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "SaleItems",
                newName: "SaleRate");

            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "Batches",
                newName: "RemainingStock");

            migrationBuilder.RenameColumn(
                name: "PurchaseStock",
                table: "Batches",
                newName: "OpeningStock");

            migrationBuilder.RenameColumn(
                name: "PurchaseId",
                table: "Batches",
                newName: "PurchaseItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_PurchaseId",
                table: "Batches",
                newName: "IX_Batches_PurchaseItemId");

            migrationBuilder.AddColumn<int>(
                name: "TotalStock",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PurchaseItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PurchaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityTaken = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleBatches_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleBatches_SaleItems_SaleItemId",
                        column: x => x.SaleItemId,
                        principalTable: "SaleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_ProductId",
                table: "PurchaseItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PurchaseId",
                table: "PurchaseItem",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleBatches_BatchId",
                table: "SaleBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleBatches_SaleItemId",
                table: "SaleBatches",
                column: "SaleItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_PurchaseItem_PurchaseItemId",
                table: "Batches",
                column: "PurchaseItemId",
                principalTable: "PurchaseItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_PurchaseItem_PurchaseItemId",
                table: "Batches");

            migrationBuilder.DropTable(
                name: "PurchaseItem");

            migrationBuilder.DropTable(
                name: "SaleBatches");

            migrationBuilder.DropColumn(
                name: "TotalStock",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "SaleRate",
                table: "SaleItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "RemainingStock",
                table: "Batches",
                newName: "Stock");

            migrationBuilder.RenameColumn(
                name: "PurchaseItemId",
                table: "Batches",
                newName: "PurchaseId");

            migrationBuilder.RenameColumn(
                name: "OpeningStock",
                table: "Batches",
                newName: "PurchaseStock");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_PurchaseItemId",
                table: "Batches",
                newName: "IX_Batches_PurchaseId");

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "SaleItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "SaleItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseRate",
                table: "Batches",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_BatchId",
                table: "SaleItems",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_Purchases_PurchaseId",
                table: "Batches",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Batches_BatchId",
                table: "SaleItems",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
