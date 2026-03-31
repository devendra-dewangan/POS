using POS.Data;
using POS.Models;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace POS.Services
{
    public class BatchService : IBatchService
    {
        private readonly AppDbContext _context;

        public BatchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Batch> AddBatchAsync(int purchaseId, decimal stock, decimal purchaseStock, decimal purchaseRate, decimal mrp)
        {
            var batch = new Batch
            {
                PurchaseId = purchaseId,
                Stock = stock,
                PurchaseStock = purchaseStock,
                PurchaseRate = purchaseRate,
                MRP = mrp
            };

            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();
            return batch;
        }

        public async Task<IEnumerable<Batch>> GetBatchesByPurchaseAsync(int purchaseId)
        {
            return await _context.Batches
                .Where(b => b.PurchaseId == purchaseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Batch>> GetAllBatchesAsync()
        {
            return await _context.Batches.ToListAsync();
        }

        public async Task<bool> BulkAddBatchesAsync(List<Batch> batches)
        {
            try
            {
                await _context.BulkInsertAsync(batches,new BulkConfig
                {
                    PreserveInsertOrder = true,
                    SetOutputIdentity = true
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during bulk insert: {ex.Message}");
                // Log the exception (ex) here as needed
                return false;
            }
        }

        public async Task<bool> SaleFromBatchAsync(int batchId, decimal quantity)
        {
            var batch = await _context.Batches.FindAsync(batchId);
            if (batch == null || batch.Stock < quantity)
            {
                return false; // Not enough stock or batch not found
            }

            batch.Stock -= quantity;
            _context.Batches.Update(batch);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}