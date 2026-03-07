using POS.Models;
using POS.Services.ImportModels;

namespace POS.Services.Import
{
    public class ImportDataProcessor
    {
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;

        public ImportDataProcessor(IProductService productService, ISupplierService supplierService)
        {
            _productService = productService;
            _supplierService = supplierService;
        }

        public async Task<List<Purchase>> ProcessPurchaseDataFromTempTable(List<ImportPurchaseTemp> tempRecords)
        {
            // 1️⃣ Extract distinct supplier names
            var supplierNames = tempRecords
                .Select(x => x.SupplierName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            // 2️⃣ Extract distinct barcodes
            var barcodes = tempRecords
                .Select(x => x.Barcode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            // 3️⃣ Query database ONCE
            var suppliersFromDb = await _supplierService.GetSuppliersByNamesAsync(supplierNames);
            var productsFromDb = await _productService.GetProductsByBarcodesAsync(barcodes);

            // 4️⃣ Build dictionaries
            var supplierCache = suppliersFromDb
                .ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

            var productCache = productsFromDb
                .ToDictionary(p => p.Barcode, StringComparer.OrdinalIgnoreCase);

            // 5️⃣ Create purchases
            var purchases = tempRecords
                .GroupBy(p => p.InvoiceNo)
                .Select(group =>
                {
                    var first = group.First();

                    // Resolve supplier
                    if (!supplierCache.TryGetValue(first.SupplierName, out var supplier))
                    {
                        supplier = new Supplier
                        {
                            Name = first.SupplierName
                        };

                        supplierCache[first.SupplierName] = supplier;
                    }

                    return new Purchase
                    {
                        InvoiceNumber = group.Key,
                        PurchaseDate = first.InvoiceDate,
                        Supplier = supplier,

                        PurchaseItems = [.. group.Select(b =>
                        {
                            // Resolve product
                            if (!productCache.TryGetValue(b.Barcode, out var product))
                            {
                                product = new Product
                                {
                                    ProductName = b.ProductName,
                                    ProductCode = b.ProductCode,
                                    Barcode = b.Barcode
                                };

                                productCache[b.Barcode] = product;
                            }

                            return new Batch
                            {
                                Product = product,
                                Stock = b.Quantity,
                                PurchaseStock = b.Quantity,
                                PurchaseRate = b.PurchaseRate,
                                MRP = b.PurchaseRate,
                                SaleRate = b.PurchaseRate
                            };
                        })]
                    };
                })
                .ToList();

            return purchases;
        }


        public List<ValidationError> ValidateData(ImportPurchaseTemp record, int rowCount)
        {
            var errors = new List<ValidationError>();

            // Check required fields
            if (string.IsNullOrEmpty(record.InvoiceNo))
                errors.Add(new ValidationError(rowCount, "InvoiceNo is required"));

            if (string.IsNullOrEmpty(record.SupplierName))
                errors.Add(new ValidationError(rowCount, "SupplierName is required"));

            if (string.IsNullOrEmpty(record.ProductName))
                errors.Add(new ValidationError(rowCount, "ProductName is required"));

            if (record.Quantity <= 0)
                errors.Add(new ValidationError(rowCount, "Quantity must be greater than 0"));

            if (record.PurchaseRate < 0)
                errors.Add(new ValidationError(rowCount, "PurchaseRate cannot be negative"));

            if (record.TotalAmount < 0)
                errors.Add(new ValidationError(rowCount, "TotalAmount cannot be negative"));

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


        public async Task<List<string>> ValidateRowDataAsync(PurchaseExcelRow rowData)
        {
            var errors = new List<string>();

            // Check required fields
            if (string.IsNullOrEmpty(rowData.InvoiceNo))
                errors.Add("InvoiceNo is required");

            if (string.IsNullOrEmpty(rowData.SupplierName))
                errors.Add("SupplierName is required");

            if (string.IsNullOrEmpty(rowData.ProductName))
                errors.Add("ProductName is required");

            if (rowData.Quantity <= 0)
                errors.Add("Quantity must be greater than 0");

            if (rowData.PurchaseRate < 0)
                errors.Add("PurchaseRate cannot be negative");

            if (rowData.TotalAmount < 0)
                errors.Add("TotalAmount cannot be negative");

            return errors;
        }
    }
}
