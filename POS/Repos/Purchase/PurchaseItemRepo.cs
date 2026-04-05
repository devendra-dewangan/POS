using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Entity;

namespace POS.Repos;

public class PurchaseItemRepo : IPurchaseItemRepo
{
    private AppDbContext _context;
    public PurchaseItemRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PurchaseItem value)
    {
        await _context.PurchaseItems.AddAsync(value);
    }

    public async Task AddBulkAsync(IEnumerable<PurchaseItem> values)
    {
        await _context.PurchaseItems.AddRangeAsync(values);
    }

    public Task DeleteAsync(PurchaseItem value)
    {
        return Task.Run(()=> true);
    }

    public async Task<IEnumerable<PurchaseItem>?> GetAllAsync()
    {
        return await _context.PurchaseItems.ToListAsync();
    }

    public Task<PurchaseItem?> GetByIDAsync(int id)
    {
        return _context.PurchaseItems.FirstOrDefaultAsync(x=>x.Id == id);
    }

    public Task UpdateAsync(PurchaseItem value)
    {
        return Task.Run(()=>true);
    }

}