using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Entity
{
    [Table("ImportPurchaseTemp")]
    public class ImportPurchaseTemp
    {
        [Key]
        public int Id { get; set; }
        
        // Excel row data fields
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string TaxType { get; set; } = string.Empty;
        public string SupplierInvoiceNo { get; set; } = string.Empty;
        public DateTime SupplierInvoiceDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string HSNCode { get; set; } = string.Empty;
        public decimal PurchaseRate { get; set; }
        public decimal Quantity { get; set; }
        public string UOM { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal CGSTPercent { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTPercent { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTPercent { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CESSPercent { get; set; }
        public decimal CESSAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string ReverseCharges { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Info { get; set; } = string.Empty;
        public string BatchSerial { get; set; } = string.Empty;
        public string MfgDate { get; set; } = string.Empty;
        public string ExpDate { get; set; } = string.Empty;
        public string IMEI1 { get; set; } = string.Empty;
        public string IMEI2 { get; set; } = string.Empty;

        public ImportStatus Status { get; set; } = ImportStatus.NotStarted;

        [Required]
        public int ImportId { get; set; }

        public ImportInfo ImportInfo { get; set; } = null!;
    }
}