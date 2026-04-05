using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Entity;

namespace POS.Repos
{
    public class ProductRepo : IProductRepo
    {
        private readonly AppDbContext _context;
        public ProductRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Product value)
        {
            await _context.Products.AddAsync(value);
        }

        public async Task AddBulkAsync(IEnumerable<Product> values)
        {
            await _context.Products.AddRangeAsync(values);
        }

        public async Task<Product?> GetByIDAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task UpdateAsync(Product value)
        {
            return Task.Run(() =>true);
        }

        public Task DeleteAsync(Product value)
        {
            return Task.Run(() =>true);
        }

        public async Task<IEnumerable<Product>?> GetByBarcodeAsync(string barcode)
        {
            return await _context.Products.Where(x => x.Barcode == barcode).ToListAsync();
        }

        public async Task<IEnumerable<Product>?> GetByNameAsync(string name)
        {
            return await _context.Products.Where(x => x.ProductName.Contains(name)).ToListAsync();
        }

        public async Task<IEnumerable<Product>?> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Products.Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<IEnumerable<Product>?> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<IEnumerable<Product>?> GetByBarcodesAsync(IEnumerable<string> barcodes)
        {
            if (barcodes == null || barcodes.Any())
                return [];

            return await _context.Products
                .Where(p => barcodes.Contains(p.Barcode))
                .ToListAsync();
        }
    }
}