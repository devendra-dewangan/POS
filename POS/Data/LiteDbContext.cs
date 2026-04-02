using LiteDB;
using POS.Models;

namespace POS.Data;
public class LiteDbContext
{
    private readonly LiteDatabase _db;

    public LiteDbContext(LiteDatabase db)
    {
        _db = db;
    }

    public ILiteCollection<SaleCart> SaleCarts =>
        _db.GetCollection<SaleCart>("saleCarts");
    
    public ILiteCollection<PurchaseCart> PurchaseCarts =>
        _db.GetCollection<PurchaseCart>("purchaseCarts");
}