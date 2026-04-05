using LiteDB;
using POS.Data;
using POS.Entity;

namespace POS.Repos;

public class LiteStore : ILiteStore
{
    private readonly LiteDbContext _context;
    public LiteStore(LiteDbContext context)
    {
        _context = context;
    }

    private ILiteCollection<SaleCart>? _saleCarts;
    public ILiteCollection<SaleCart> SaleCarts => _saleCarts ??= _context.SaleCarts;

    private ILiteCollection<PurchaseCart>? _purchaseCarts;
    public ILiteCollection<PurchaseCart> PurchaseCarts => _purchaseCarts ??= _context.PurchaseCarts;
}