using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Repos
{
    public class BatchRepo : IBatchRepo
    {
        private AppDbContext _context;
        public BatchRepo(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddAsync(Batch value)
        {
            await _context.Batches.AddAsync(value);
        }

        public async Task AddBulkAsync(IEnumerable<Batch> values)
        {
            await _context.Batches.AddRangeAsync(values);

        }

        public Task DeleteAsync(Batch value)
        {
            return Task.Run(()=> true);
        }

        public async Task<IEnumerable<Batch>?> GetAllAsync()
        {
            return await _context.Batches.ToListAsync();
        }

        public Task<Batch?> GetByIDAsync(int id)
        {
            return _context.Batches.FirstOrDefaultAsync(x=>x.Id == id);
        }

        public Task UpdateAsync(Batch value)
        {
           return Task.Run(()=>true);
        }
    }
}
