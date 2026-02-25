using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;
using POS.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace POS.Controllers
{
    // Helper classes for import processing
    public class ImportResult
    {
        public int ProcessedRows { get; set; }
        public int ProductsAdded { get; set; }
        public int SuppliersAdded { get; set; }
        public int BatchesAdded { get; set; }
        public int PurchasesAdded { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class ExcelRowData
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

    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ExcelReaderService _excelReaderService;
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly IPurchaseService _purchaseService;
        private readonly IBatchService _batchService;

        public ImportController(
            AppDbContext context, 
            ExcelReaderService excelReaderService,
            IProductService productService,
            ISupplierService supplierService,
            IPurchaseService purchaseService,
            IBatchService batchService)
        {
            _context = context;
            _excelReaderService = excelReaderService;
            _productService = productService;
            _supplierService = supplierService;
            _purchaseService = purchaseService;
            _batchService = batchService;
        }

        // POST: api/import/purchase
        [HttpPost("purchase")]
        public async Task<ActionResult> ImportPurchaseExcel()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                // Read Excel file using the service
                var excelData = await _excelReaderService.ReadExcelFileAsync(file);

                if (excelData.Count < 2)
                {
                    return BadRequest("Excel file must contain header row and at least one data row.");
                }

                // Process the Excel data
                var result = await ProcessPurchaseData(excelData);

                return Ok(new
                {
                    message = "Purchase data imported successfully",
                    fileName = file.FileName,
                    processedRows = result.ProcessedRows,
                    productsAdded = result.ProductsAdded,
                    suppliersAdded = result.SuppliersAdded,
                    batchesAdded = result.BatchesAdded,
                    purchasesAdded = result.PurchasesAdded,
                    errors = result.Errors,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing file: {ex.Message}");
            }
        }

        private async Task<ImportResult> ProcessPurchaseData(List<Dictionary<string, string>> excelData)
        {
            var result = new ImportResult();
            var purchaseGroups = new Dictionary<string, List<ExcelRowData>>();

            // Group rows by purchase (same invoice number)
            for (int i = 1; i < excelData.Count; i++) // Skip header row
            {
                var row = excelData[i];
                var invoiceNo = GetCellValue(row, "Column0"); // Invoice No.
                var invoiceDate = GetCellValue(row, "Column1"); // Invoice Date
                var supplierName = GetCellValue(row, "Column2"); // Supplier Name

                if (string.IsNullOrEmpty(invoiceNo) || string.IsNullOrEmpty(supplierName))
                {
                    result.Errors.Add($"Row {i + 1}: Missing required Invoice No. or Supplier Name");
                    continue;
                }

                var key = $"{invoiceNo}_{supplierName}";
                if (!purchaseGroups.ContainsKey(key))
                {
                    purchaseGroups[key] = new List<ExcelRowData>();
                }

                purchaseGroups[key].Add(new ExcelRowData
                {
                    InvoiceNo = invoiceNo,
                    InvoiceDate = invoiceDate,
                    SupplierName = supplierName,
                    ProductName = GetCellValue(row, "Column3"), // Product Name
                    Quantity = GetDecimalValue(row, "Column4"), // Qty.
                    UOM = GetCellValue(row, "Column5"), // UOM
                    PurchaseRate = GetDecimalValue(row, "Column6"), // Purchase Rate
                    MRP = GetDecimalValue(row, "Column7") // MRP
                });
            }

            // Process each purchase group
            foreach (var group in purchaseGroups)
            {
                var rows = group.Value;
                if (rows.Count == 0) continue;

                var firstRow = rows[0];
                
                // 1. Add Supplier if not exists
                var supplier = await _supplierService.GetOrCreateSupplierAsync(firstRow.SupplierName);
                result.SuppliersAdded++;

                // 2. Add Purchase record
                var purchaseDate = DateTime.TryParse(firstRow.InvoiceDate, out var date) ? date : DateTime.Now;
                var purchase = await _purchaseService.AddPurchaseAsync(supplier.Id, firstRow.InvoiceNo, purchaseDate);
                result.PurchasesAdded++;

                // 3. Process each product in the purchase
                foreach (var rowData in rows)
                {
                    if (string.IsNullOrEmpty(rowData.ProductName)) continue;

                    // Add Product if not exists
                    var product = await _productService.GetOrCreateProductAsync(
                        rowData.ProductName, 
                        productCode: "", 
                        barcode: "");
                    result.ProductsAdded++;

                    // Add Batch
                    var batch = await _batchService.AddBatchAsync(
                        purchase.Id, 
                        rowData.Quantity, 
                        rowData.Quantity, // PurchaseStock equals Quantity
                        rowData.PurchaseRate, 
                        rowData.MRP);
                    result.BatchesAdded++;
                }
            }

            result.ProcessedRows = excelData.Count - 1; // Exclude header
            return result;
        }

        private string GetCellValue(Dictionary<string, string> row, string columnKey)
        {
            return row.ContainsKey(columnKey) ? row[columnKey] : "";
        }

        private decimal GetDecimalValue(Dictionary<string, string> row, string columnKey)
        {
            var value = GetCellValue(row, columnKey);
            return decimal.TryParse(value, out var result) ? result : 0;
        }

        // POST: api/import/sale
        [HttpPost("sale")]
        public async Task<ActionResult> ImportSaleExcel()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // For now, just return success and file path
                // You can process the Excel file later
                return Ok(new
                {
                    message = "Sale file uploaded successfully",
                    fileName = file.FileName,
                    filePath = filePath,
                    uploadTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        // GET: api/import/status
        [HttpGet("status")]
        public ActionResult GetImportStatus()
        {
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                return Ok(new { files = new List<object>() });
            }

            var files = Directory.GetFiles(uploadsDir)
                .Select(f => new
                {
                    name = Path.GetFileName(f),
                    path = f,
                    size = new FileInfo(f).Length,
                    created = new FileInfo(f).CreationTime
                })
                .ToList();

            return Ok(new { files = files });
        }
    }
}