using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIdToBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_Products_ProductId",
                table: "Batches");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_Products_ProductId",
                table: "Batches",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_Products_ProductId",
                table: "Batches");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_Products_ProductId",
                table: "Batches",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
