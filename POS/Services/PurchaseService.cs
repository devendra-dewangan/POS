using POS.Data;
using POS.Models;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
namespace POS.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly AppDbContext _context;

        public PurchaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Purchase> AddPurchaseAsync(int supplierId, string invoiceNumber, DateTime purchaseDate)
        {
            var purchase = new Purchase
            {
                SupplierId = supplierId,
                InvoiceNumber = invoiceNumber,
                PurchaseDate = purchaseDate
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            return purchase;
        }

        public async Task<Purchase?> GetPurchaseByInvoiceAsync(string invoiceNumber)
        {
            return await _context.Purchases
                .FirstOrDefaultAsync(p => p.InvoiceNumber.ToLower() == invoiceNumber.ToLower());
        }

        public async Task<IEnumerable<Purchase>> GetAllPurchasesAsync()
        {
            return await _context.Purchases.ToListAsync();
        }

        public async Task<bool> AddPurchaseBulkAsync(IEnumerable<Purchase> purchases)
        {
            try
            {
                await _context.BulkInsertAsync(purchases, new BulkConfig
                {
                    PreserveInsertOrder = true,
                    SetOutputIdentity = true
                });
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                System.Console.WriteLine($"Error adding purchases in bulk: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Purchase>> GetPurchasesByInvoiceNumbersAsync(IEnumerable<string> invoiceNumbers)
        {
            if (invoiceNumbers == null || invoiceNumbers.Any() == false)
                return [];

            return await _context.Purchases
                .Where(p => invoiceNumbers.Contains(p.InvoiceNumber))
                .ToListAsync();
        }
    }
}