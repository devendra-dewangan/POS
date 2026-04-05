using POS.Entity;
using POS.Model;

namespace POS.Services
{
    public interface IPurchaseService
    {
        Task<int> AddPurchaseAsync(int supplierId);
        Task<IEnumerable<Purchase>?> GetPurchaseByInvoiceAsync(string invoiceNumber);
        Task<Purchase?> AddPurchaseItemAsync(int purchaseDraftId, AddPurchaseItemRequestDto request);
        Task<Purchase> CompletePurchaseAsync(int purchaseCartId);
        Task<IEnumerable<Purchase>> GetAllPurchasesAsync();
    }
}