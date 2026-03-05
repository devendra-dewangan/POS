using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AddImportPurchaseTempTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportPurchaseTemp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImportId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceNo = table.Column<string>(type: "TEXT", nullable: false),
                    InvoiceDate = table.Column<string>(type: "TEXT", nullable: false),
                    TaxType = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierInvoiceNo = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierInvoiceDate = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierName = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    GSTIN = table.Column<string>(type: "TEXT", nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    HSNCode = table.Column<string>(type: "TEXT", nullable: false),
                    PurchaseRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    UOM = table.Column<string>(type: "TEXT", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CGSTPercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    SGSTPercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IGSTPercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CESSPercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    CESSAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReverseCharges = table.Column<string>(type: "TEXT", nullable: false),
                    ProductCode = table.Column<string>(type: "TEXT", nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", nullable: false),
                    Colour = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<string>(type: "TEXT", nullable: false),
                    Info = table.Column<string>(type: "TEXT", nullable: false),
                    BatchSerial = table.Column<string>(type: "TEXT", nullable: false),
                    MfgDate = table.Column<string>(type: "TEXT", nullable: false),
                    ExpDate = table.Column<string>(type: "TEXT", nullable: false),
                    IMEI1 = table.Column<string>(type: "TEXT", nullable: false),
                    IMEI2 = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportPurchaseTemp", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportPurchaseTemp");
        }
    }
}
