using NetTopologySuite.Index.HPRtree;
using POS.Entity;
using POS.Repos;

namespace POS.Services
{
    public class ImportDataProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public ImportDataProcessor(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<PurchaseItem>> ProcessPurchaseDataFromTempTable(IEnumerable<ImportPurchaseTemp> tempRecords)
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

            // Extract distinct purchase
            var invoices = tempRecords
                .Select(x => x.InvoiceNo)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            // 3️⃣ Query database ONCE
            var suppliersFromDb = await _unitOfWork.Suppliers.GetByNamesAsync(supplierNames);
            var productsFromDb = await _unitOfWork.Products.GetByBarcodesAsync(barcodes);
            var purchasesFromDb = await _unitOfWork.Purchases.GetByInvoiceNumbersAsync(invoices);

            // 4️⃣ Build dictionaries
            var supplierCache = (suppliersFromDb ?? [])
                .ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

            var productCache = (productsFromDb ?? [])
                .ToDictionary(p => p.Barcode, StringComparer.OrdinalIgnoreCase);

            var purchaseCache = (purchasesFromDb ?? [])
                .ToDictionary(p => p.InvoiceNumber, StringComparer.OrdinalIgnoreCase);

            var purchaseItems = tempRecords.Select(record =>
            {
                if (!supplierCache.TryGetValue(record.SupplierName, out var supplier))
                {
                    supplier = new Supplier
                    {
                        Name = record.SupplierName
                    };

                    supplierCache[record.SupplierName] = supplier;
                }

                if (!purchaseCache.TryGetValue(record.InvoiceNo, out var purchase))
                {
                    
                    purchase = new Purchase
                    {
                        InvoiceNumber = record.InvoiceNo,
                        PurchaseDate = record.InvoiceDate,
                        Supplier = supplier
                    };

                    purchaseCache[record.InvoiceNo] = purchase;
                }

                if(!productCache.TryGetValue(record.Barcode, out var product))
                {
                    product = new Product
                    {
                        ProductName = record.ProductName,
                        ProductCode = record.ProductCode,
                        Barcode = record.Barcode
                    };

                    productCache[record.Barcode] = product;
                }

                product.TotalStock += record.Quantity;

                return new PurchaseItem()
                {
                    Product = product,
                    Purchase = purchase,
                    Quantity = record.Quantity,
                    PurchaseRate = record.PurchaseRate,
                    Batches =
                    [
                        new() {
                            Product = product,
                            OpeningStock = record.Quantity,
                            RemainingStock = record.Quantity,
                            MRP = record.PurchaseRate,
                            SaleRate = record.PurchaseRate,
                        }
                    ],
                };
            });

            var newProduct = productCache.Where(x => x.Value.Id == 0).Select(x=>x.Value);
            if (newProduct.Any())
            {
                await _unitOfWork.Products.AddBulkAsync(newProduct);
                _logger.LogInformation("adding new product" + newProduct.Count(i => i.Id == 0));
            }

            var newSup = supplierCache.Where(x => x.Value.Id == 0).Select(x=>x.Value);
            if (newSup.Any())
            {
                await _unitOfWork.Suppliers.AddBulkAsync(newSup);
                _logger.LogInformation("adding new supplier" + newSup.Count(i => i.Id == 0));

            }

            if (newProduct.Any() || newSup.Any())
            {
                var saved = await _unitOfWork.CommitAsync();
                _logger.LogInformation($"Saved {saved} rows");

            }

            var newPurcahse = purchaseCache.Where(x => x.Value.Id == 0)
                                .Select(x=> x.Value);
            if (newPurcahse.Any())
            {
                foreach (var purchase in newPurcahse)
                {
                    purchase.SupplierId = purchase.Supplier!.Id;
                    purchase.Supplier = null;
                }
                await _unitOfWork.Purchases.AddBulkAsync(newPurcahse);
                _logger.LogInformation("adding new purchase" + newPurcahse.Count(i => i.Id == 0));

                var saved = await _unitOfWork.CommitAsync();
                _logger.LogInformation($"Added {saved} rows");

            }

            foreach (var item in purchaseItems)
            {
                item.PurchaseId = item.Purchase!.Id;
                item.Purchase = null;

                item.ProductId = item.Product!.Id;

                foreach(var batch in item.Batches)
                {
                    batch.ProductId = batch.Product!.Id;
                    batch.Product = null;
                }
            }

            return purchaseItems;
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
}
