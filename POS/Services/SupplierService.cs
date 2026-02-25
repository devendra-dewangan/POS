using POS.Data;
using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    }
}