using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Entity;

namespace POS.Repos
{
    public class SaleItemRepo : ISaleItemRepo
    {
        private AppDbContext _context;
        public SaleItemRepo(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddAsync(SaleItem value)
        {
            await _context.SaleItems.AddAsync(value);
        }

        public async Task AddBulkAsync(IEnumerable<SaleItem> values)
        {
            await _context.SaleItems.AddRangeAsync(values);

        }

        public Task DeleteAsync(SaleItem value)
        {
            return Task.Run(()=> true);
        }

        public async Task<IEnumerable<SaleItem>?> GetAllAsync()
        {
            return await _context.SaleItems.ToListAsync();
        }

        public Task<SaleItem?> GetByIDAsync(int id)
        {
            return _context.SaleItems.FirstOrDefaultAsync(x=>x.Id == id);
        }

        public Task UpdateAsync(SaleItem value)
        {
           return Task.Run(()=>true);
        }
    }
}
