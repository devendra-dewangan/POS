using POS.Data;
using POS.Models;
using POS.Services.ImportModels;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace POS.Services.Import
{
    public class ImportService : IImportService
    {
        private readonly AppDbContext _context;
        private readonly ExcelReaderService _excelReaderService;
        private readonly ImportDataProcessor _dataProcessor;
        private readonly IPurchaseService _purchaseService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        private readonly IBatchService _batchService;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<int, List<ValidationError>> _validationErrors;
        private readonly List<ImportError> _errorList;
        private ImportInfo _importInfo;
        private const int BatchSizeErrors = 100;
        private const int BatchSize = 5000;

        private readonly ILogger<ImportService> _logger;

        public ImportService(
            AppDbContext context,
            ExcelReaderService excelReaderService,
            IProductService productService,
            ISupplierService supplierService,
            IPurchaseService purchaseService,
            IBatchService batchService,
            ILogger<ImportService> logger
            )
        {
            _context = context;
            _excelReaderService = excelReaderService;
            _supplierService = supplierService;
            _productService = productService;
            _purchaseService = purchaseService;
            _batchService = batchService;
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();
            _validationErrors = new Dictionary<int, List<ValidationError>>();
            _errorList = new List<ImportError>();
            _importInfo = new ImportInfo();
            _dataProcessor = new ImportDataProcessor(_productService, _supplierService, _purchaseService, _logger);
        }

        public async Task<bool> ImportPurchaseDataAsync(string filepath)
        {
            _importInfo.ImportType = ImportType.Purchase;
            _importInfo.FileName = Path.GetFileName(filepath);
            _importInfo.TotalRecords = await _excelReaderService.GetTotalRows(filepath);

            await CreateImportInfoAsync(_importInfo);

            try
            {
                await _excelReaderService.ReadExcelAsync<PurchaseExcelRow>(
                    filepath,
                    BatchSize,
                    // batch handler
                    ProcessBatch,
                    // error handler
                    HandleBatchError,
                    // progress handler
                    HandleBatchProgress,
                    _cancellationTokenSource.Token
                );

                if (_validationErrors.Count != 0 || _errorList.Count != 0)
                {
                    _logger.LogInformation($"Import completed with {_validationErrors.Count} records having validation errors.");
                    await DeleteImportDataAsync(_importInfo.Id);
                    return false;
                }

                // Process data with validation and transaction handling
                return await ProcessDataWithValidationAndTransaction(_importInfo.Id);


            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError($"Error importing purchase data: {ex.Message}");
                await DeleteImportDataAsync(_importInfo.Id);
                return false;
            }
        }

        private void HandleBatchProgress(ImportProgress progress)
        {
            _logger.LogInformation($"Processed rows: {progress.ProcessedRows} , Total rows percentage: {progress.ProcessedRows / (double)_importInfo.TotalRecords * 100:F2}%");
        }

        private void HandleBatchError(ImportError error)
        {
            _errorList.Add(error);
            _logger.LogError($"Row {error.RowNumber} error: {error.Message}");
            if (_errorList.Count > BatchSizeErrors)
            {
                _logger.LogWarning("Too many errors, cancelling import.");
                _cancellationTokenSource.Cancel();
            }
        }

        private async Task ProcessBatch(List<PurchaseExcelRow> rowDataBatch, int rowNumber)
        {
            var tempRecords = new List<ImportPurchaseTemp>();
            var rowCount = rowNumber;

            foreach (var rowData in rowDataBatch)
            {
                try
                {
                    rowCount++;
                    var tempRecord = new ImportPurchaseTemp
                    {
                        ImportId = _importInfo.Id,
                        InvoiceNo = rowData.InvoiceNo,
                        InvoiceDate = rowData.InvoiceDate,
                        TaxType = rowData.TaxType,
                        SupplierInvoiceNo = rowData.SupplierInvoiceNo,
                        SupplierInvoiceDate = rowData.SupplierInvoiceDate,
                        SupplierName = rowData.SupplierName,
                        State = rowData.State,
                        GSTIN = rowData.GSTIN,
                        ProductName = rowData.ProductName,
                        HSNCode = rowData.HSNCode,
                        PurchaseRate = rowData.PurchaseRate,
                        Quantity = rowData.Quantity,
                        UOM = rowData.UOM,
                        DiscountPercent = rowData.DiscountPercent,
                        DiscountAmount = rowData.DiscountAmount,
                        CGSTPercent = rowData.CGSTPercent,
                        CGSTAmount = rowData.CGSTAmount,
                        SGSTPercent = rowData.SGSTPercent,
                        SGSTAmount = rowData.SGSTAmount,
                        IGSTPercent = rowData.IGSTPercent,
                        IGSTAmount = rowData.IGSTAmount,
                        CESSPercent = rowData.CESSPercent,
                        CESSAmount = rowData.CESSAmount,
                        TotalAmount = rowData.TotalAmount,
                        ReverseCharges = rowData.ReverseCharges,
                        ProductCode = rowData.ProductCode,
                        Barcode = rowData.Barcode,
                        Colour = rowData.Colour,
                        Size = rowData.Size,
                        Info = rowData.Info,
                        BatchSerial = rowData.BatchSerial,
                        MfgDate = rowData.MfgDate,
                        ExpDate = rowData.ExpDate,
                        IMEI1 = rowData.IMEI1,
                        IMEI2 = rowData.IMEI2
                    };

                    var recordValidationErrors = _dataProcessor.ValidateData(tempRecord, rowCount);
                    if (recordValidationErrors.Count == 0)
                    {
                        tempRecords.Add(tempRecord);
                        continue;
                    }

                    _validationErrors[tempRecord.Id] = recordValidationErrors;
                    if (_validationErrors.Count > BatchSizeErrors)
                    {
                        _logger.LogError($"Batch has {_validationErrors.Count} records with validation errors. Skipping batch.");
                        _cancellationTokenSource.Cancel();
                    }
                }
                catch (Exception ex)
                {
                    // Log error and skip this record
                    _logger.LogError($"Error processing Excel batch: {ex.Message}");
                }
            }

            try
            {
                if (_validationErrors.Count != 0)
                {
                    _logger.LogError($"Batch has {_validationErrors.Count} records with validation errors. Skipping batch.");
                    return;
                }

                _logger.LogInformation($"Batch {rowNumber / BatchSize}: Saving {tempRecords.Count} records to database.");
                // Save batch to database
                await _context.BulkInsertAsync(tempRecords);
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError($"Error saving batch to database: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ImportSaleDataAsync(string filePath)
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

        private async Task<bool> ProcessDataWithValidationAndTransaction(int importId)
        {

            // Use transaction only for the final processing and saving
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                for (int i = 0; i < Math.Ceiling((double)_importInfo.TotalRecords / BatchSize); i++)
                {
                    // Get next batch of pending records
                    var batchRecords = await _context.ImportPurchaseTemp
                        .Where(t => t.ImportId == importId && t.Status == ImportStatus.NotStarted)
                        .Skip(i * BatchSize)
                        .Take(BatchSize)
                        .ToListAsync();

                    // Process the batch
                    var processedRecords =
                        await _dataProcessor.ProcessPurchaseDataFromTempTable(batchRecords);


                    var suppliers = processedRecords
                                    .Select(p => p.Purchase?.Supplier)
                                    .Where(s => s != null && s.Id == 0)
                                    .Select(s => s!)
                                    .Distinct() //by reference
                                    .Distinct().ToList();


                    // save supplier
                    if (suppliers.Count != 0)
                    {
                        var isSupplierAdded = await _supplierService.BulkAddSuppliersAsync(suppliers);
                        if (!isSupplierAdded)
                        {
                            // Processing failed - rollback transaction
                            _logger.LogError($"Batch {i + 1}: Failed to add suppliers. Rolling back transaction.");
                            await transaction.RollbackAsync();
                            return false;
                        }
                        _logger.LogInformation($"Batch {i + 1}: Added {suppliers.Count} suppliers.");
                    }

                    var products = processedRecords
                        .Select(i => i.Product)
                        .Where(p => p != null && p.Id == 0)
                        .Select(p => p!)
                        .Distinct() //by reference
                        .ToList();

                    //save product
                    if (products.Count != 0)
                    {
                        var isProductAdded = await _productService.BulkAddProductsAsync(products);
                        if (!isProductAdded)
                        {
                            // Processing failed - rollback transaction
                            _logger.LogError($"Batch {i + 1}: Failed to add products. Rolling back transaction.");
                            await transaction.RollbackAsync();
                            return false;
                        }
                        _logger.LogInformation($"Batch {i + 1}: Added {products.Count} products.");
                    }

                    var invoices = processedRecords.Select(p => p.Purchase)
                        .Where(p => p != null && p.Id == 0)
                        .Select(p => p!)
                        .Distinct() //by reference
                        .ToList();

                    if (invoices.Count != 0)
                    {

                        invoices.ForEach(inv =>
                        {
                            inv.SupplierId = inv.Supplier != null ? inv.Supplier.Id : inv.SupplierId;
                        });

                        var savedInvoices = await _purchaseService.AddPurchaseBulkAsync(invoices);
                        if (!savedInvoices)
                        {
                            // Processing failed - rollback transaction
                            _logger.LogError($"Batch {i + 1}: Failed to add invoices. Rolling back transaction.");
                            await transaction.RollbackAsync();
                            return false;
                        }
                        _logger.LogInformation($"Batch {i + 1}: Added {invoices.Count} invoices.");
                    }

                    processedRecords.ForEach(r =>
                    {
                        r.PurchaseId = r.Purchase != null ? r.Purchase.Id : r.PurchaseId;
                        r.ProductId = r.Product != null ? r.Product.Id : r.ProductId;
                    });


                    //save purchase 
                    var saveResult = await _batchService.BulkAddBatchesAsync(processedRecords);
                    if (!saveResult)
                    {
                        // Processing failed - rollback transaction
                        _logger.LogError($"Batch {i + 1}: Failed to save purchases. Rolling back transaction.");
                        await transaction.RollbackAsync();
                        return false;
                    }
                    _logger.LogInformation($"Batch {i + 1}: Saved {processedRecords.Count} purchases.");

                    // Update temp records status to Processed
                    batchRecords.ForEach(r => r.Status = ImportStatus.Completed);
                    await _context.BulkUpdateAsync(batchRecords);
                    _logger.LogInformation($"Batch {i + 1}: Updated temp records status to Completed.");

                }


                // Commit the transaction if all processing is successful
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                _logger.LogError($"Error processing data with transaction: {ex.Message}");
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteImportDataAsync(int importId)
        {
            var records = await _context.ImportPurchaseTemp
            .Where(t => t.ImportId == importId)
            .ExecuteDeleteAsync();
            return records > 0;
        }

        public async Task<int> CreateImportInfoAsync(ImportInfo importInfo)
        {
            await _context.ImportInfos.AddAsync(importInfo);
            return await _context.SaveChangesAsync();
        }
    }
}
