using System;

namespace POS.Services.ImportModels
{
    public class PurchaseExcelRow
    {
        // Exact column order from Excel headers
        public string InvoiceNo { get; set; } = string.Empty;           // Column 0
        public string InvoiceDate { get; set; } = string.Empty;         // Column 1
        public string TaxType { get; set; } = string.Empty;             // Column 2
        public string SupplierInvoiceNo { get; set; } = string.Empty;   // Column 3
        public string SupplierInvoiceDate { get; set; } = string.Empty; // Column 4
        public string SupplierName { get; set; } = string.Empty;        // Column 5
        public string State { get; set; } = string.Empty;               // Column 6
        public string GSTIN { get; set; } = string.Empty;               // Column 7
        public string ProductName { get; set; } = string.Empty;         // Column 8
        public string HSNCode { get; set; } = string.Empty;             // Column 9
        public decimal PurchaseRate { get; set; }                       // Column 10
        public decimal Quantity { get; set; }                           // Column 11
        public string UOM { get; set; } = string.Empty;                 // Column 12
        public decimal DiscountPercent { get; set; }                    // Column 13
        public decimal DiscountAmount { get; set; }                     // Column 14
        public decimal CGSTPercent { get; set; }                        // Column 15
        public decimal CGSTAmount { get; set; }                         // Column 16
        public decimal SGSTPercent { get; set; }                        // Column 17
        public decimal SGSTAmount { get; set; }                         // Column 18
        public decimal IGSTPercent { get; set; }                        // Column 19
        public decimal IGSTAmount { get; set; }                         // Column 20
        public decimal CESSPercent { get; set; }                        // Column 21
        public decimal CESSAmount { get; set; }                         // Column 22
        public decimal TotalAmount { get; set; }                        // Column 23
        public string ReverseCharges { get; set; } = string.Empty;      // Column 24
        public string ProductCode { get; set; } = string.Empty;         // Column 25
        public string Barcode { get; set; } = string.Empty;             // Column 26
        public string Colour { get; set; } = string.Empty;              // Column 27
        public string Size { get; set; } = string.Empty;                // Column 28
        public string Info { get; set; } = string.Empty;                // Column 29
        public string BatchSerial { get; set; } = string.Empty;         // Column 30
        public string MfgDate { get; set; } = string.Empty;             // Column 31
        public string ExpDate { get; set; } = string.Empty;             // Column 32
        public string IMEI1 { get; set; } = string.Empty;               // Column 33
        public string IMEI2 { get; set; } = string.Empty;               // Column 34
    }
}