using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POS.Data;
using POS.Models;
using Microsoft.EntityFrameworkCore;

namespace POS.Services.Import
{
    public class ImportDataProcessor
    {
        private readonly AppDbContext _context;

        public ImportDataProcessor(AppDbContext context)
        {
            _context = context;
        }

        // Event to emit batch count information
        public event Action<int, int> BatchCountUpdated;

        public async Task<List<ValidationError>> ValidateTempDataAsync(Guid importId)
        {
            try
            {
                var validationErrors = await ValidateTempData(importId);
                if (validationErrors.Any())
                {
                    // Mark records with errors
                    foreach (var error in validationErrors)
                    {
                        var record = await _context.ImportPurchaseTemp.FindAsync(error.RecordId);
                        if (record != null)
                        {
                            record.Status = "Failed";
                            record.ErrorMessage = error.ErrorMessage;
                            _context.ImportPurchaseTemp.Update(record);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                
                return validationErrors;
            }
            catch (Exception)
            {
                return new List<ValidationError>();
            }
        }

        public async Task<(List<Supplier>, List<Product>, List<Purchase>, List<Batch>, List<ImportPurchaseTemp>)> ProcessPurchaseDataFromTempTable(List<ImportPurchaseTemp> tempRecords)
        {
            // Prepare lists for bulk saving
            var suppliers = new List<Supplier>();
            var products = new List<Product>();
            var purchases = new List<Purchase>();
            var batches = new List<Batch>();
            var processedTempRecords = new List<ImportPurchaseTemp>();

            // ----------------------------
            // 1️⃣ Preload Suppliers
            // ----------------------------
            var supplierNames = tempRecords
                .Select(x => x.SupplierName)
                .Distinct()
                .ToList();

            var existingSuppliers = await _context.Suppliers
                .Where(s => supplierNames.Contains(s.Name))
                .ToListAsync();

            var supplierDict = existingSuppliers
                .ToDictionary(s => s.Name, s => s);

            // ----------------------------
            // 2️⃣ Preload Products
            // ----------------------------
            var productNames = tempRecords
                .Where(x => !string.IsNullOrEmpty(x.ProductName))
                .Select(x => x.ProductName)
                .Distinct()
                .ToList();

            var existingProducts = await _context.Products
                .Where(p => productNames.Contains(p.ProductName))
                .ToListAsync();

            var productDict = existingProducts
                .ToDictionary(p => p.ProductName, p => p);

            foreach (var group in tempRecords.GroupBy(x => new { x.InvoiceNo, x.SupplierName }))
            {
                var first = group.First();

                // ----------------------------
                // 3️⃣ Get or Create Supplier
                // ----------------------------
                if (!supplierDict.TryGetValue(first.SupplierName, out var supplier))
                {
                    supplier = new Supplier { Name = first.SupplierName };
                    suppliers.Add(supplier);
                    supplierDict[first.SupplierName] = supplier;
                }

                // ----------------------------
                // 4️⃣ Check Purchase Duplicate
                // ----------------------------
                var existingPurchase = await _context.Purchases
                    .FirstOrDefaultAsync(p =>
                        p.InvoiceNumber == first.InvoiceNo &&
                        p.SupplierId == supplier.Id);

                if (existingPurchase != null)
                {
                    foreach (var r in group)
                    {
                        r.Status = "Completed";
                        r.ProcessedAt = DateTime.UtcNow;
                        r.ErrorMessage = "Duplicate Purchase";
                        processedTempRecords.Add(r);
                    }
                    continue;
                }

                // ----------------------------
                // 5️⃣ Create Purchase
                // ----------------------------
                var purchaseDate = DateTime.TryParse(first.InvoiceDate, out var dt)
                    ? dt
                    : DateTime.UtcNow;

                var purchase = new Purchase
                {
                    SupplierId = supplier.Id,
                    InvoiceNumber = first.InvoiceNo,
                    PurchaseDate = purchaseDate
                };

                purchases.Add(purchase);

                // ----------------------------
                // 6️⃣ Process Products
                // ----------------------------
                foreach (var record in group)
                {
                    if (string.IsNullOrEmpty(record.ProductName))
                        continue;

                    if (!productDict.TryGetValue(record.ProductName, out var product))
                    {
                        product = new Product
                        {
                            ProductName = record.ProductName,
                            ProductCode = record.ProductCode,
                            Barcode = record.Barcode
                        };

                        products.Add(product);
                        productDict[record.ProductName] = product;
                    }

                    // Safe MRP calculation
                    var mrp = record.Quantity > 0
                        ? record.TotalAmount / record.Quantity
                        : 0;

                    var batch = new Batch
                    {
                        ProductId = product.Id,
                        PurchaseId = purchase.Id,
                        Stock = record.Quantity,
                        PurchaseStock = record.Quantity,
                        PurchaseRate = record.PurchaseRate,
                        MRP = mrp
                    };

                    batches.Add(batch);

                    record.Status = "Completed";
                    record.ProcessedAt = DateTime.UtcNow;
                    processedTempRecords.Add(record);
                }
            }

            // Emit batch count information
            OnBatchCountUpdated(suppliers.Count, products.Count);

            return (suppliers, products, purchases, batches, processedTempRecords);
        }


        private async Task<List<ValidationError>> ValidateTempData(Guid importId)
        {
            var errors = new List<ValidationError>();
            var records = await _context.ImportPurchaseTemp
                .Where(t => t.ImportId == importId)
                .ToListAsync();

            foreach (var record in records)
            {
                // Check required fields
                if (string.IsNullOrEmpty(record.InvoiceNo))
                    errors.Add(new ValidationError(record.Id, "InvoiceNo is required"));

                if (string.IsNullOrEmpty(record.SupplierName))
                    errors.Add(new ValidationError(record.Id, "SupplierName is required"));

                if (string.IsNullOrEmpty(record.ProductName))
                    errors.Add(new ValidationError(record.Id, "ProductName is required"));

                if (record.Quantity <= 0)
                    errors.Add(new ValidationError(record.Id, "Quantity must be greater than 0"));

                if (record.PurchaseRate < 0)
                    errors.Add(new ValidationError(record.Id, "PurchaseRate cannot be negative"));

                if (record.TotalAmount < 0)
                    errors.Add(new ValidationError(record.Id, "TotalAmount cannot be negative"));
            }

            return errors;
        }

        // Helper class for validation errors
        public class ValidationError
        {
            public int RecordId { get; set; }
            public string ErrorMessage { get; set; }

            public ValidationError(int recordId, string errorMessage)
            {
                RecordId = recordId;
                ErrorMessage = errorMessage;
            }
        }

        // Event invoker for batch count updates
        protected virtual void OnBatchCountUpdated(int supplierCount, int productCount)
        {
            BatchCountUpdated?.Invoke(supplierCount, productCount);
        }
    }
}