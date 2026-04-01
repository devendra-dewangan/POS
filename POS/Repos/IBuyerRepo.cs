using POS.Models;

namespace POS.Repos
{
    public interface IBuyerRepo : IRepository<Buyer>
    {
        Task<IEnumerable<Buyer>?> GetByNameAsync(string name);
    }
}