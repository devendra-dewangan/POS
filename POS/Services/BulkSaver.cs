using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Services
{
    public interface IBulkSaver
    {
        Task<bool> SavePurchaseDataAsync(
            List<Supplier> suppliers,
            List<Product> products,
            List<Purchase> purchases,
            List<Batch> batches,
            List<ImportPurchaseTemp> tempRecords);
    }

    public class BulkSaver : IBulkSaver
    {
        private readonly AppDbContext _context;

        public BulkSaver(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SavePurchaseDataAsync(
            List<Supplier> suppliers,
            List<Product> products,
            List<Purchase> purchases,
            List<Batch> batches,
            List<ImportPurchaseTemp> tempRecords)
        {
            try
            {
                // Save all entities at once in a single operation
                if (suppliers.Any())
                    await _context.Suppliers.AddRangeAsync(suppliers);
                
                if (products.Any())
                    await _context.Products.AddRangeAsync(products);
                
                if (purchases.Any())
                    await _context.Purchases.AddRangeAsync(purchases);
                
                if (batches.Any())
                    await _context.Batches.AddRangeAsync(batches);
                
                if (tempRecords.Any())
                {
                    foreach (var record in tempRecords)
                    {
                        record.Status = "Completed";
                        record.ProcessedAt = DateTime.UtcNow;
                    }
                    _context.ImportPurchaseTemp.UpdateRange(tempRecords);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}