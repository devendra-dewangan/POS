using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;
using SQLitePCL;

namespace POS.Repos
{
    public class PurchaseRepo : IPurchaseRepo
    {
        private readonly AppDbContext _context;
        public PurchaseRepo(AppDbContext context)
        {
            _context = context;          
        }

        public async Task AddAsync(Purchase value)
        {
            await _context.Purchases.AddAsync(value);
        }

        public async Task AddBulkAsync(IEnumerable<Purchase> values)
        {
            await _context.Purchases.AddRangeAsync(values);
        }

        public Task DeleteAsync(Purchase value)
        {
            return Task.Run(()=>true);
        }

        public async Task<IEnumerable<Purchase>?> GetAllAsync()
        {
            return await _context.Purchases.ToListAsync();
        }

        public async Task<Purchase?> GetByIDAsync(int id)
        {
            return await _context.Purchases.FirstOrDefaultAsync(x=> x.Id == id);
        }

        public Task UpdateAsync(Purchase value)
        {
            return Task.Run(()=>true);
        }
    }
}