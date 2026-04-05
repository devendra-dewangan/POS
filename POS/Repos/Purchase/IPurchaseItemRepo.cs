namespace POS.Repos;
using POS.Entity;

public interface IPurchaseItemRepo : IRepository<PurchaseItem>,IAddBulk<PurchaseItem>
{
}