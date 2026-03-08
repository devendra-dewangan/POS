using POS.Data;
using POS.Models;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace POS.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly AppDbContext _context;

        public SupplierService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Supplier> AddSupplierAsync(string supplierName)
        {
            var supplier = new Supplier
            {
                Name = supplierName
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<Supplier> GetOrCreateSupplierAsync(string supplierName)
        {
            // Try to find existing supplier by name
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Name.ToLower() == supplierName.ToLower());

            if (supplier != null)
            {
                return supplier;
            }

            // Create new supplier
            supplier = new Supplier
            {
                Name = supplierName
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<Supplier?> GetSupplierByNameAsync(string supplierName)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Name.ToLower() == supplierName.ToLower());
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersByNamesAsync(List<string> supplierNames)
        {
            if (supplierNames == null || supplierNames.Count == 0)
                return [];

            return await _context.Suppliers
                .Where(s => supplierNames.Contains(s.Name))
                .ToListAsync();
        }

        public async Task<bool> BulkAddSuppliersAsync(List<Supplier> suppliers)
        {
            try
            {
                if (suppliers == null || suppliers.Count == 0)
                    return false;

                await _context.BulkInsertAsync(suppliers);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error during bulk insert: {ex.Message}");
                return false;
            }
            if (suppliers == null || suppliers.Count == 0)
                return false;

            await _context.BulkInsertAsync(suppliers);
        }   
    }
}