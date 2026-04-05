using POS.Entity;

namespace POS.Repos
{
    public interface IBatchRepo : IRepository<Batch>, IAddBulk<Batch>
    {
         Task<IEnumerable<Batch>?> GetByPurchaseIdAsync(int purchaseId);
    }
}