using POS.Entity;
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

        public async Task<int> AddPurchaseAsync(int supplierId, string invoiceNumber, DateTime purchaseDate)
        {

            var purchase = new PurchaseCart
            {
                Purchase = new Purchase
                {
                    SupplierId = supplierId,
                    InvoiceNumber = invoiceNumber,
                    PurchaseDate = purchaseDate
                }
            };
            _liteStore.PurchaseCarts.Upsert(purchase);
            return purchase.Id;
        }

        public async Task<Purchase> CompletePurchaseAsync(int purchaseCartId)
        {
            var purchaseCart = _liteStore.PurchaseCarts.FindById(purchaseCartId);
            if (purchaseCart == null || purchaseCart.Status == CartStatus.Completed)
                throw new InvalidOperationException("Invalid purchase cart.");

            if (purchaseCart.Items.Count == 0)
                throw new InvalidOperationException("Purchase cart is empty.");

            // Here you would typically save the purchase to the main database
            var purchase = purchaseCart.Purchase!;
            await _unitOfWork.Purchases.AddAsync(purchase);
            await _unitOfWork.CommitAsync();

            var batches = purchaseCart.Items;
            batches.ForEach(item => item.PurchaseId = purchase.Id); // Update the cart with the saved purchase (with ID)
            await _unitOfWork.Batches.AddBulkAsync(batches);
            await _unitOfWork.CommitAsync();

            // Update the cart status
            purchaseCart.Status = CartStatus.Completed;
            _liteStore.PurchaseCarts.Update(purchaseCart);
            _liteStore.PurchaseCarts.Delete(purchaseCartId);
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