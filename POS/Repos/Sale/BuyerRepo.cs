using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Entity;

namespace POS.Repos
{
    public class BuyerRepo : IBuyerRepo
    {
        private AppDbContext _context;
        public BuyerRepo(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddAsync(Buyer value)
        {
            await _context.Buyers.AddAsync(value);
        }

        public async Task AddBulkAsync(IEnumerable<Buyer> values)
        {
            await _context.Buyers.AddRangeAsync(values);

        }

        public Task DeleteAsync(Buyer value)
        {
            return Task.Run(()=> true);
        }

        public async Task<IEnumerable<Buyer>?> GetAllAsync()
        {
            return await _context.Buyers.ToListAsync();
        }

        public async Task<IEnumerable<Buyer>?> GetByNameAsync(string name)
        {
            return await _context.Buyers.Where(x=>x.Name.Contains(name)).ToListAsync();
        }

        public Task<Buyer?> GetByIDAsync(int id)
        {
            return _context.Buyers.FirstOrDefaultAsync(x=>x.Id == id);
        }

        public Task UpdateAsync(Buyer value)
        {
           return Task.Run(()=>true);
        }
    }
}
