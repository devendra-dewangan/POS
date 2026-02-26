using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POS.Data;
using POS.Models;
using POS.Services.ImportModels;
using Microsoft.AspNetCore.Http;

namespace POS.Services
{
    public class ImportService : IImportService
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly IPurchaseService _purchaseService;
        private readonly IBatchService _batchService;
        private readonly ExcelReaderService _excelReaderService;

        public ImportService(
            AppDbContext context,
            IProductService productService,
            ISupplierService supplierService,
            IPurchaseService purchaseService,
            IBatchService batchService,
            ExcelReaderService excelReaderService)
        {
            _context = context;
            _productService = productService;
            _supplierService = supplierService;
            _purchaseService = purchaseService;
            _batchService = batchService;
            _excelReaderService = excelReaderService;
        }

        public async Task<bool> ImportPurchaseDataAsync(IFormFile file)
        {
            try
            {
                var excelData = new List<PurchaseExcelRow>();
                var processingCompleted = false;
                var processingSuccess = false;
                var processingError = "";

                // Subscribe to ExcelReaderService events
                _excelReaderService.RowDataEmitted += (rowData) =>
                {
                    if (rowData is PurchaseExcelRow purchaseRow)
                    {
                        excelData.Add(purchaseRow);
                    }
                };

                _excelReaderService.ProcessingCompleted += (success, message) =>
                {
                    processingCompleted = true;
                    processingSuccess = success;
                    processingError = message;
                };

                // Define column mapping for purchase data based on PurchaseExcelRow model
                var columnMapping = new Dictionary<int, string>
                {
                    { 0, "InvoiceNo" },           // Column 0: Invoice No.
                    { 1, "InvoiceDate" },         // Column 1: Invoice Date
                    { 2, "TaxType" },             // Column 2: Tax Type
                    { 3, "SupplierInvoiceNo" },   // Column 3: Supplier's Invoice No.
                    { 4, "SupplierInvoiceDate" }, // Column 4: Supplier's Invoice Date
                    { 5, "SupplierName" },        // Column 5: Supplier Name
                    { 6, "State" },               // Column 6: State
                    { 7, "GSTIN" },               // Column 7: GSTIN
                    { 8, "ProductName" },         // Column 8: Product Name
                    { 9, "HSNCode" },             // Column 9: HSN Code
                    { 10, "PurchaseRate" },       // Column 10: Purchase Rate
                    { 11, "Quantity" },           // Column 11: Qty.
                    { 12, "UOM" },                // Column 12: UOM
                    { 13, "DiscountPercent" },    // Column 13: Discount %
                    { 14, "DiscountAmount" },     // Column 14: Discount
                    { 15, "CGSTPercent" },        // Column 15: CGST %
                    { 16, "CGSTAmount" },         // Column 16: CGST
                    { 17, "SGSTPercent" },        // Column 17: SGST %
                    { 18, "SGSTAmount" },         // Column 18: SGST/UTGST
                    { 19, "IGSTPercent" },        // Column 19: IGST %
                    { 20, "IGSTAmount" },         // Column 20: IGST
                    { 21, "CESSPercent" },        // Column 21: CESS %
                    { 22, "CESSAmount" },         // Column 22: CESS
                    { 23, "TotalAmount" },        // Column 23: Total Amount
                    { 24, "ReverseCharges" },     // Column 24: Reverse Charges
                    { 25, "ProductCode" },        // Column 25: Product Code
                    { 26, "Barcode" },            // Column 26: Barcode
                    { 27, "Colour" },             // Column 27: Colour
                    { 28, "Size" },               // Column 28: Size
                    { 29, "Info" },               // Column 29: Info
                    { 30, "BatchSerial" },        // Column 30: Batch/Serial
                    { 31, "MfgDate" },            // Column 31: Mfg Date
                    { 32, "ExpDate" },            // Column 32: Exp Date
                    { 33, "IMEI1" },              // Column 33: IMEI-1
                    { 34, "IMEI2" }               // Column 34: IMEI-2
                };

                // 1. Parse Excel file using ExcelReaderService
                await _excelReaderService.ReadExcelFileAsync<PurchaseExcelRow>(file, columnMapping);

                // Wait for processing to complete
                while (!processingCompleted)
                {
                    await Task.Delay(100);
                }

                if (!processingSuccess)
                {
                    return false;
                }

                if (excelData.Count < 1) // Header row is not included in generic mapping
                {
                    return false;
                }

                // 2. Process purchase data
                var result = await ProcessPurchaseData(excelData);
                
                return result;
            }
            catch (Exception)
            {
                // Log error
                return false;
            }
        }

        private async Task<bool> ProcessPurchaseData(List<PurchaseExcelRow> excelData)
        {
            try
            {
                // Group rows by purchase (same invoice number and supplier)
                var purchaseGroups = new Dictionary<string, List<PurchaseExcelRow>>();

                // Group rows by purchase (same invoice number and supplier)
                foreach (var row in excelData)
                {
                    var invoiceNo = row.InvoiceNo;
                    var invoiceDate = row.InvoiceDate;
                    var supplierName = row.SupplierName;

                    if (string.IsNullOrEmpty(invoiceNo) || string.IsNullOrEmpty(supplierName))
                    {
                        continue;
                    }

                    var key = $"{invoiceNo}_{supplierName}";
                    if (!purchaseGroups.ContainsKey(key))
                    {
                        purchaseGroups[key] = new List<PurchaseExcelRow>();
                    }

                    purchaseGroups[key].Add(row);
                }

                // Process each purchase group
                foreach (var group in purchaseGroups)
                {
                    var rows = group.Value;
                    if (rows.Count == 0) continue;

                    var firstRow = rows[0];

                    // 1. Create supplier if not exists
                    var supplier = await _supplierService.GetOrCreateSupplierAsync(firstRow.SupplierName);

                    // 2. Create purchase entry
                    var purchaseDate = DateTime.TryParse(firstRow.InvoiceDate, out var date) ? date : DateTime.Now;
                    var purchase = await _purchaseService.AddPurchaseAsync(
                        supplier.Id, 
                        firstRow.InvoiceNo, 
                        purchaseDate);

                    // 3. Process each product in the purchase
                    foreach (var rowData in rows)
                    {
                        if (string.IsNullOrEmpty(rowData.ProductName)) continue;

                        // Create product if not exists
                        var product = await _productService.GetOrCreateProductAsync(
                            rowData.ProductName, 
                            rowData.ProductCode, 
                            rowData.Barcode);

                        // Create batch for the product
                        var batch = await _batchService.AddBatchAsync(
                            purchase.Id,
                            rowData.Quantity, // Stock
                            rowData.Quantity, // PurchaseStock (use same as Quantity)
                            rowData.PurchaseRate, // PurchaseRate
                            rowData.TotalAmount / rowData.Quantity // MRP (calculate from TotalAmount and Quantity)
                        );
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ImportSaleDataAsync(IFormFile file)
        {
            try
            {
                // TODO: Implement sale data import logic
                // This will read Excel files and process sale data
                // - Parse Excel file using ExcelReaderService
                // - Create/update buyers
                // - Create sales
                // - Create sale items
                // - Update batch stock levels

                return true;
            }
            catch (Exception)
            {
                // Log error
                return false;
            }
        }

        // Helper class for processing Excel row data
        private class ExcelRowData
        {
            public string InvoiceNo { get; set; } = string.Empty;
            public string InvoiceDate { get; set; } = string.Empty;
            public string SupplierName { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public string UOM { get; set; } = string.Empty;
            public decimal PurchaseRate { get; set; }
            public decimal MRP { get; set; }
        }
    }
}
