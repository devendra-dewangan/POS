using POS.Entity;
using POS.Repos;

namespace POS.Services
{
    public class ImportService : IImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILiteStore _liteStore;
        private readonly ExcelReaderService _excelReaderService;
        private readonly ImportDataProcessor _dataProcessor;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<int, List<ValidationError>> _validationErrors;
        private readonly List<ImportError> _errorList;
        private ImportInfo _importInfo;
        private readonly ILogger<ImportService> _logger;
        
        private const int BatchSizeErrors = 100;
        private const int BatchSize = 5000;


        public ImportService(ILogger<ImportService> logger, IUnitOfWork unitOfWork, ILiteStore liteStore)
        {
            _unitOfWork = unitOfWork;
            _liteStore = liteStore;
            _logger = logger;

            _dataProcessor = new ImportDataProcessor(unitOfWork, _logger);
            _excelReaderService = new ExcelReaderService();
            _cancellationTokenSource = new CancellationTokenSource();
            _validationErrors = new Dictionary<int, List<ValidationError>>();
            _errorList = new List<ImportError>();
            _importInfo = new ImportInfo();
        }

        public Task<bool> ImportPurchaseDataAsync(string filepath)
        {
            return ImportPurchaseDataAsync(filepath, $"Import completed with {_validationErrors.Count} records having validation errors.");
        }


        public async Task<bool> ImportPurchaseDataAsync(string filepath, string message)
        {
            _importInfo.ImportType = ImportType.Purchase;
            _importInfo.FileName = Path.GetFileName(filepath);
            _importInfo.TotalRecords = await _excelReaderService.GetTotalRows(filepath);

            await CreateImportInfoAsync(_importInfo);

            try
            {
                await _excelReaderService.ReadExcelAsync<PurchaseExcelRow>(
                    new ImportFileInfo
                    {
                        FilePath = filepath,
                        BatchSize =  BatchSize,
                        TotalRows = _importInfo.TotalRecords,
                        DatetimeFormat = "dd-MM-yyyy HH:mm:ss"
                    },
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
                    _logger.LogInformation(message);
                    await DeleteImportDataAsync(_importInfo.Id);
                    return false;
                }

                // Process data with validation and transaction handling
                return await ProcessDataWithValidationAndTransaction();
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

                // Save batch to database

                
                _liteStore.ImportInfos.InsertBulk(tempRecords.Select(tempRecord => new ImportCart
                {
                    Item = tempRecord,
                    ImportId = _importInfo.Id
                }));
                _logger.LogInformation($"Batch {rowNumber / BatchSize}: Saved {tempRecords.Count} records to temporary database.");
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

        private async Task<bool> ProcessDataWithValidationAndTransaction()
        {

            // Use transaction only for the final processing and saving
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {

                for (int i = 0; i < Math.Ceiling((double)_importInfo.TotalRecords / BatchSize); i++)
                {
                    // Get next batch of pending records
                    var batchRecords = _liteStore.ImportInfos
                        .Query()
                        .Where(t => t.ImportId == _importInfo.Id)
                        .Select(t => t.Item)
                        .Skip(i * BatchSize)
                        .Limit(BatchSize)
                        .ToList();

                    // Process the batch
                    var processedRecords =
                        await _dataProcessor.ProcessPurchaseDataFromTempTable(batchRecords!);

                    var newSupplier = processedRecords.Where(pi=> pi.Purchase!.Supplier!.Id == 0)
                                        .Select(pi=> pi.Purchase!.Supplier).Distinct();
                    if (newSupplier.Any())
                    {
                        await _unitOfWork.Suppliers.AddBulkAsync(newSupplier!);
                        await _unitOfWork.CommitAsync();
                    }

                    var newPurchase = processedRecords.Where(pi=> pi.Purchase!.Id == 0)
                                        .Select(pi=> {
                                            pi.Purchase!.SupplierId = pi.Purchase.Supplier!.Id;
                                            return pi.Purchase;}).Distinct();
                    if (newPurchase.Any())
                    {
                        await _unitOfWork.Purchases.AddBulkAsync(newPurchase!);
                        await _unitOfWork.CommitAsync();
                    }
                    // Save processed data to main tables
                    await _unitOfWork.PurchaseItems.AddBulkAsync(processedRecords);
                    await _unitOfWork.CommitAsync();
                    _logger.LogInformation($"Batch {i + 1} processed and saved successfully.");

                }
                // Commit the transaction if all processing is successful
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                await DeleteImportDataAsync(_importInfo.Id);
                _logger.LogError($"Error processing data with transaction: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteImportDataAsync(int importId)
        {
            return _liteStore.ImportInfos.DeleteMany(t => t.ImportId == importId) > 0;
        }

        public async Task<int> CreateImportInfoAsync(ImportInfo importInfo)
        {
            await _unitOfWork.ImportInfos.AddAsync(importInfo);
            return await _unitOfWork.CommitAsync();
        }
    }
}
