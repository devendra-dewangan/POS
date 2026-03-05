using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POS.Data;
using POS.Models;
using POS.Services.ImportModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace POS.Services.Import
{
    public class ImportService : IImportService
    {
        private readonly AppDbContext _context;
        private readonly ExcelReaderService _excelReaderService;
        private readonly ImportDataProcessor _dataProcessor;
        private readonly ImportDataCleanup _dataCleanup;
        private readonly IBulkSaver _bulkSaver;

        public ImportService(
            AppDbContext context,
            ExcelReaderService excelReaderService,
            IBulkSaver bulkSaver)
        {
            _context = context;
            _excelReaderService = excelReaderService;
            _dataProcessor = new ImportDataProcessor(_context);
            _dataCleanup = new ImportDataCleanup(_context);
            _bulkSaver = bulkSaver;
        }

        public async Task<bool> ImportPurchaseDataAsync(IFormFile file)
        {
            var importId = Guid.NewGuid();

            try
            {
                await _excelReaderService.ReadExcelAsync<PurchaseExcelRow>(
                            file,
                // batch handler
                async batch =>
                {
                    await ProcessBatch(batch, importId, file);
                },

                // error handler
                error =>
                {
                    Console.WriteLine($"Row {error.RowNumber} error: {error.Message}");
                    _dataCleanup.DeleteImportDataAsync(importId).Wait();
                },

                // progress handler
                progress =>
                {
                    Console.WriteLine($"Processed rows: {progress.ProcessedRows}");
                });

                // 2. Validate temporary data first - only proceed if validation passes
                var validationErrors = await _dataProcessor.ValidateTempDataAsync(importId);
                if (validationErrors.Any())
                {
                    // Validation failed - clean up temporary data
                    await _dataCleanup.DeleteImportDataAsync(importId);
                    return false;
                }

                // Use transaction only for the final processing and saving
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 3. Process temporary data in batches until all data is processed
                    const int batchSize = 1000;

                    while (true)
                    {
                        // Get next batch of pending records
                        var batchRecords = await _context.ImportPurchaseTemp
                            .Where(t => t.ImportId == importId && t.Status == "Pending")
                            .OrderBy(t => t.Id)
                            .Take(batchSize)
                            .ToListAsync();

                        if (!batchRecords.Any())
                        {
                            break; // No more records to process
                        }

                        // Process the batch
                        var (suppliers, products, purchases, batches, processedRecords) =
                            await _dataProcessor.ProcessPurchaseDataFromTempTable(batchRecords);

                        // Save the processed data using BulkSaver
                        var saveResult = await _bulkSaver.SavePurchaseDataAsync(
                            suppliers, products, purchases, batches, processedRecords);

                        if (!saveResult)
                        {
                            // Processing failed - rollback transaction
                            await transaction.RollbackAsync();
                            return false;
                        }
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
            catch (Exception)
            {
                // Log error
                await _dataCleanup.DeleteImportDataAsync(importId);
                return false;
            }
        }


        private async Task ProcessBatch(List<PurchaseExcelRow> rowDataBatch, Guid importId, IFormFile file)
        {
            var tempRecords = new List<ImportPurchaseTemp>();

            foreach (var rowData in rowDataBatch)
            {
                try
                {
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

                    tempRecords.Add(tempRecord);
                }
                catch (Exception ex)
                {
                    // Log error and skip this record
                    Console.WriteLine($"Error processing Excel batch: {ex.Message}");
                }
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

        public async Task<bool> DeleteImportDataAsync(Guid importId)
        {
            return await _dataCleanup.DeleteImportDataAsync(importId);
        }
    }
}