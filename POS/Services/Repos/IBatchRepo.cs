using POS.Models;

namespace POS.Repos
{
    public interface IBatchRepo : IRepository<Batch>
    {
         Task<IEnumerable<Batch>?> GetByPurchaseIdAsync(int purchaseId);
    }
}