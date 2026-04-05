using POS.Entity;
using POS.Model;
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

        public async Task<int> AddPurchaseAsync(int supplierId)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIDAsync(supplierId) 
                ?? throw new InvalidOperationException("Supplier not found.");

            var purchase = new PurchaseCart
            {
                Purchase = new Purchase
                {
                    Supplier = supplier
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

            var purchase = purchaseCart.Purchase!;
            var purchaseItems = purchase.PurchaseItems;
            var productIds = purchaseItems.Select(i => i.ProductId).Distinct().ToList();
            var products = await _unitOfWork.Products.GetByIdsAsync(productIds);
            var productDict = products!.ToDictionary(p => p.Id);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                purchase.PurchaseDate = DateTime.UtcNow;
                purchase.InvoiceNumber = $"INV-{DateTime.UtcNow.Ticks}";

                foreach (var item in purchaseItems)
                {
                    // Ensure relationship
                    item.Purchase = purchase;

                    foreach (var batch in item.Batches)
                    {
                        batch.ProductId = item.ProductId;
                        batch.PurchaseItem = item;
                    }

                    if (!productDict!.TryGetValue(item.ProductId, out var product))
                        throw new Exception($"Product not found: {item.ProductId}");

                    product.TotalStock += item.Quantity;
                }

                await _unitOfWork.Purchases.AddAsync(purchase);
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // Update the cart status
            purchaseCart.Status = CartStatus.Completed;
            _liteStore.PurchaseCarts.Update(purchaseCart);
            _liteStore.PurchaseCarts.Delete(purchaseCartId);
            return purchase;
        }

        public async Task<IEnumerable<Purchase>> GetAllPurchasesAsync()
        {
            return await _unitOfWork.Purchases.GetAllAsync() ?? [];
        }


        public async Task<IEnumerable<Purchase>> GetPurchasesByInvoiceNumbersAsync(IEnumerable<string> invoiceNumbers)
        {
            return await _unitOfWork.Purchases.GetByInvoiceNumbersAsync(invoiceNumbers) ?? [];
        }

        public async Task<IEnumerable<Purchase>?> GetPurchaseByInvoiceAsync(string invoiceNumber)
        {
            var purchases = await _unitOfWork.Purchases.GetByInvoiceNumberAsync(invoiceNumber);
            return purchases ?? [];
        }

        public async Task<Purchase?> AddPurchaseItemAsync(int purchaseDraftId, AddPurchaseItemRequestDto request)
        {
            var purchaseCart = _liteStore.PurchaseCarts.FindById(purchaseDraftId);

            if (purchaseCart == null || purchaseCart.Status == CartStatus.Completed)
                throw new InvalidOperationException("Invalid purchase cart.");

            var purchase = purchaseCart.Purchase!;

            var product = await _unitOfWork.Products.GetByIDAsync(request.ProductId)
                ?? throw new InvalidOperationException("Product not found.");

            // ✅ Validation
            if (request.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            if (request.UnitPrice <= 0)
                throw new InvalidOperationException("Invalid price.");

            // 🔥 Check if item already exists
            var existingItem = purchase.PurchaseItems
                .FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem != null)
            {
                // 👉 Option 1: Update existing
                purchase.PurchaseItems.Remove(existingItem);
            }

            // ✅ Create new PurchaseItem
            var purchaseItem = new PurchaseItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                PurchaseRate = request.UnitPrice,
                Batches = []
            };

            // ✅ Add batches
            if (request.Batches != null && request.Batches.Any())
            {
                foreach (var batchDto in request.Batches)
                {
                    purchaseItem.Batches.Add(new Batch
                    {
                        BatchNumber = batchDto.BatchNumber,
                        RemainingStock = batchDto.Quantity,
                        OpeningStock = batchDto.Quantity,
                        MRP = batchDto.MRP,
                        SaleRate = request.UnitPrice,
                        ProductId = request.ProductId,        
                    });
                }
            }
            else
            {
                purchaseItem.Batches.Add(new Batch
                {
                    BatchNumber = $"BATCH-{DateTime.UtcNow.Ticks}",
                    RemainingStock = request.Quantity,
                    OpeningStock = request.Quantity,
                    MRP = request.UnitPrice,
                    SaleRate = request.UnitPrice,
                    ProductId = request.ProductId,          
                });
            }

            // ✅ Add to purchase
            purchase.PurchaseItems.Add(purchaseItem);

            // Save in LiteDB (draft)
            _liteStore.PurchaseCarts.Update(purchaseCart);

            return purchase;
        }
    }
}