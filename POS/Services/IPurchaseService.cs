using POS.Models;

namespace POS.Services
{
    public interface IPurchaseService
    {
        Task<Purchase> AddPurchaseAsync(int supplierId, string invoiceNumber, DateTime purchaseDate);
        Task<Purchase?> GetPurchaseByInvoiceAsync(string invoiceNumber);
        Task<IEnumerable<Purchase>> GetAllPurchasesAsync();
    }
}