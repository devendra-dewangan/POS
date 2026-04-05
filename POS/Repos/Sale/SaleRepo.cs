using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Entity;

namespace POS.Repos
{
    public class SaleRepo : ISaleRepo
    {
        private AppDbContext _context;
        public SaleRepo(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddAsync(Sale value)
        {
            await _context.Sales.AddAsync(value);
        }

        public async Task AddBulkAsync(IEnumerable<Sale> values)
        {
            await _context.Sales.AddRangeAsync(values);

        }

        public Task DeleteAsync(Sale value)
        {
            return Task.Run(()=> true);
        }

        public async Task<IEnumerable<Sale>?> GetAllAsync()
        {
            return await _context.Sales.ToListAsync();
        }

        public Task<Sale?> GetByIDAsync(int id)
        {
            return _context.Sales.FirstOrDefaultAsync(x=>x.Id == id);
        }

        public Task UpdateAsync(Sale value)
        {
           return Task.Run(()=>true);
        }

        public async Task<IEnumerable<Sale>?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _context.Sales
                .Where(p => p.InvoiceNumber.Contains(invoiceNumber))
                .ToListAsync();
        }
    }
}
