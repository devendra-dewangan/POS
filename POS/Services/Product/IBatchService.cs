using POS.Models;

namespace POS.Services
{
    public interface IBatchService
    {
        Task<Batch> AddBatchAsync(int purchaseCartId, decimal purchaseStock, decimal purchaseRate, decimal mrp, int productId);
        Task<IEnumerable<Batch>> GetBatchesByPurchaseAsync(int purchaseId);
        Task<IEnumerable<Batch>> GetAllBatchesAsync();

        Task<bool> SaleFromBatchAsync(int batchId, decimal quantity);

        Task<bool>BulkAddBatchesAsync(List<Batch> batches);
    }
}