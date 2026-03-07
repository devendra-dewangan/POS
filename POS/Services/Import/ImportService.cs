using POS.Data;
using POS.Models;
using POS.Services.ImportModels;
using Microsoft.EntityFrameworkCore;
using static POS.Services.Import.ImportDataProcessor;

namespace POS.Services.Import
{
    public class ImportService : IImportService
    {
        private readonly AppDbContext _context;
        private readonly ExcelReaderService _excelReaderService;
        private readonly ImportDataProcessor _dataProcessor;
        private const int BatchSizeErrors = 100;
        private const int BatchSize = 1000;

        public ImportService(
            AppDbContext context,
            ExcelReaderService excelReaderService,
            IProductService productService,
            ISupplierService supplierService)
        {
            _context = context;
            _excelReaderService = excelReaderService;
            _dataProcessor = new ImportDataProcessor(productService, supplierService);
        }

        public async Task<bool> ImportPurchaseDataAsync(IFormFile file)
        {
            var importId = Guid.NewGuid();
            var cancellationTokenSource = new CancellationTokenSource();
            var validationErrors = new Dictionary<int, List<ValidationError>>();
            var errorList = new List<ImportError>();
            try
            {
                await _excelReaderService.ReadExcelAsync<PurchaseExcelRow>(
                            file,
                // batch handler
                async (batch, rowNumber) =>
                {
                    await ProcessBatch(batch, rowNumber,importId, file, validationErrors, cancellationTokenSource);

                },

                // error handler
                error =>
                {
                    errorList.Add(error);
                    Console.WriteLine($"Row {error.RowNumber} error: {error.Message}");
                    if (errorList.Count > BatchSizeErrors)
                    {
                        Console.WriteLine("Too many errors, cancelling import.");
                        cancellationTokenSource.Cancel();
                    }
                },

                // progress handler
                progress =>
                {
                    Console.WriteLine($"Processed rows: {progress.ProcessedRows}");
                }
                ,cancellationTokenSource.Token
                );
                
                if (validationErrors.Any() || errorList.Any())
                {
                    Console.WriteLine($"Import completed with {validationErrors.Count} records having validation errors."); 
                    await DeleteImportDataAsync(importId);
                    return false;
                }

                // Process data with validation and transaction handling
                return await ProcessDataWithValidationAndTransaction(importId);


            }
            catch (Exception)
            {
                // Log error
                await DeleteImportDataAsync(importId);
                return false;
            }
        }


        private async Task ProcessBatch(List<PurchaseExcelRow> rowDataBatch, int rowNumber, Guid importId, IFormFile file, Dictionary<int, List<ValidationError>> validationErrors, CancellationTokenSource cancellationTokenSource)
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
                        ImportId = importId,
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
                        IMEI2 = rowData.IMEI2,
                        Status = "Pending",
                        FileName = file.FileName,
                        CreatedAt = DateTime.UtcNow
                    };

                    var recordValidationErrors = _dataProcessor.ValidateData(tempRecord,rowCount);
                    if (recordValidationErrors.Any())
                    {
                        validationErrors[tempRecord.Id] = recordValidationErrors;
                    }
                    else
                    {
                        tempRecords.Add(tempRecord);
                    }

                    if (validationErrors.Count > BatchSizeErrors)
                    {
                        Console.WriteLine($"Batch has {validationErrors.Count} records with validation errors. Skipping batch.");
                        cancellationTokenSource.Cancel();
                        return;
                    }

                }
                catch (Exception ex)
                {
                    // Log error and skip this record
                    Console.WriteLine($"Error processing Excel batch: {ex.Message}");
                }
            }

            try
            {
                if (validationErrors.Any())
                {
                    Console.WriteLine($"Batch has {validationErrors.Count} records with validation errors. Skipping batch.");
                    return;
                }
                // Save batch to database
                await _context.ImportPurchaseTemp.AddRangeAsync(tempRecords);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving batch to database: {ex.Message}");
                throw;
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

        private async Task<bool> ProcessDataWithValidationAndTransaction(Guid importId)
        {
            
            // Use transaction only for the final processing and saving
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                while (true)
                {
                    // Get next batch of pending records
                    var batchRecords = await _context.ImportPurchaseTemp
                        .Where(t => t.ImportId == importId && t.Status == "Pending")
                        .OrderBy(t => t.Id)
                        .Take(BatchSize)
                        .ToListAsync();

                    if (!batchRecords.Any())
                    {
                        break; // No more records to process
                    }

                    // Process the batch
                    var processedRecords =
                        await _dataProcessor.ProcessPurchaseDataFromTempTable(batchRecords);

                    // Save the processed data using BulkSaver

                    var suppliers = processedRecords
                                    .Select(p => p.Supplier)
                                    .Where(s => s != null && s.Id == 0)
                                    .Distinct().ToList();

                    // save supplier
                    var products = processedRecords.SelectMany(p => p.PurchaseItems)
                        .Select(i => i.Product)
                        .Where(p => p != null && p.Id == 0)
                        .Distinct()
                        .ToList();
                    //save product

                    processedRecords.ForEach(p =>
                    {
                        p.SupplierId = p.Supplier == null ? p.SupplierId : p.Supplier.Id;
                        foreach (var i in p.PurchaseItems)
                        {
                            i.ProductId = i.Product == null ? i.ProductId : i.Product.Id;
                        }
                    });
                    
                    //save purchase 
                    // if (!saveResult)
                    // {
                    //     // Processing failed - rollback transaction
                    //     await transaction.RollbackAsync();
                    //     return false;
                    // }
                }

                // Commit the transaction if all processing is successful
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteImportDataAsync(Guid importId)
        {
            var records = await _context.ImportPurchaseTemp
                .Where(t => t.ImportId == importId)
                .ToListAsync();

            if (!records.Any())
            {
                return false;
            }

            _context.ImportPurchaseTemp.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }
    
    }
}
