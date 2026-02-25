using POS.Data;
using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    }
}