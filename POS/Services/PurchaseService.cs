using POS.Models;
using POS.Repos;
namespace POS.Services
{
    public class PurchaseService : IPurchaseService
    {
        private IUnitOfWork _unitOfWork;
        private ILiteStore _liteStore;

        public PurchaseService(IUnitOfWork uow, ILiteStore liteStore)
        {
            _unitOfWork = uow;
            _liteStore = liteStore;
        }

        public async Task<Purchase> AddPurchaseAsync(int supplierId, string invoiceNumber, DateTime purchaseDate)
        {
            
            var purchase = new Purchase
            {
                SupplierId = supplierId,
                InvoiceNumber = invoiceNumber,
                PurchaseDate = purchaseDate
            };

            await _unitOfWork.Purchases.AddAsync(purchase);
            await _unitOfWork.CommitAsync();
            return purchase;
        }

        public async Task<Purchase?> GetPurchaseByInvoiceAsync(string invoiceNumber)
        {
            var purchases = await _unitOfWork.Purchases.GetByInvoiceNumberAsync(invoiceNumber);

            return purchases?.FirstOrDefault();
        }

        public async Task<IEnumerable<Purchase>> GetAllPurchasesAsync()
        {
            return await _unitOfWork.Purchases.GetAllAsync() ?? [];
        }

        public async Task<bool> AddPurchaseBulkAsync(IEnumerable<Purchase> purchases)
        {
            try
            {
                await _unitOfWork.Purchases.AddBulkAsync(purchases);
                await _unitOfWork.CommitAsync();
                // await _context.BulkInsertAsync(purchases, new BulkConfig
                // {
                //     PreserveInsertOrder = true,
                //     SetOutputIdentity = true
                // });
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
            return await _unitOfWork.Purchases.GetByInvoiceNumbersAsync(invoiceNumbers) ?? [];
        }
    }
}