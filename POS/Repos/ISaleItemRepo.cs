using POS.Models;

namespace POS.Repos
{
    public interface ISaleItemRepo : IRepository<SaleItem>,IAddBulk<SaleItem>
    {
        
    }
}