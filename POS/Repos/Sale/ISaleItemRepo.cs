using POS.Entity;

namespace POS.Repos
{
    public interface ISaleItemRepo : IRepository<SaleItem>,IAddBulk<SaleItem>
    {
        
    }
}