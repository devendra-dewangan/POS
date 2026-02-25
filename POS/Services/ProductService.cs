using POS.Data;
using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace POS.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product> AddProductAsync(string productName, string productCode, string barcode)
        {
            var product = new Product
            {
                ProductName = productName,
                ProductCode = productCode,
                Barcode = barcode
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetOrCreateProductAsync(string productName, string productCode = "", string barcode = "")
        {
            // Try to find existing product by name
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName.ToLower() == productName.ToLower());

            if (product != null)
            {
                return product;
            }

            // Create new product
            product = new Product
            {
                ProductName = productName,
                ProductCode = productCode,
                Barcode = barcode
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> GetProductByNameAsync(string productName)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName.ToLower() == productName.ToLower());
        }

        public async Task<Product?> GetProductByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return null;

            return await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode.ToLower() == barcode.ToLower());
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }
    }
}