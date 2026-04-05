using POS.Models;

namespace POS.Repos
{
    public interface ISupplierRepo : IRepository<Supplier>, IAddBulk<Supplier>
    {
        Task<IEnumerable<Supplier>?> GetByNameAsync(string name);
        Task<IEnumerable<Supplier>> GetByNamesAsync(IEnumerable<string> supplierNames);
    }
}