using POS.Models;

namespace POS.Services
{
    public interface IBatchService
    {
        Task<Batch> AddBatchAsync(int purchaseId, decimal stock, decimal purchaseStock, decimal purchaseRate, decimal mrp);
        Task<IEnumerable<Batch>> GetBatchesByPurchaseAsync(int purchaseId);
        Task<IEnumerable<Batch>> GetAllBatchesAsync();

        Task<bool>BulkAddBatchesAsync(List<Batch> batches);
    }
}