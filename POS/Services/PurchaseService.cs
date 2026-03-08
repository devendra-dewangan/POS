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
                await _context.BulkInsertAsync(purchases);
                return true;
            }
            catch (Exception)
            {
                // Log the exception (ex) as needed
                return false;
            }
        }
    }
}