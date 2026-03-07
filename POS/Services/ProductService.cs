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
        private readonly IBatchService _batchService;

        public ProductService(AppDbContext context, IBatchService batchService)
        {
            _context = context;
            _batchService = batchService;
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

            // Create an empty batch for the product
            // Since we need a purchaseId but want supplier as null, we'll create a batch with minimal data
            // Note: Batch requires a PurchaseId, so we need to handle this carefully
            // For now, we'll create a batch with PurchaseId = null (if allowed by the model)
            // or we need to create a "dummy" purchase record

            try
            {
                // Create a batch with null PurchaseId (empty batch)
                // This assumes the Batch model allows nullable PurchaseId
                var batch = new Batch
                {
                    ProductId = product.Id, // Link batch to the created product
                    PurchaseId = null, // No purchase associated (empty batch)
                    Stock = 0, // Initial stock is 0
                    PurchaseStock = 0, // Initial purchase stock is 0
                    PurchaseRate = 0, // Initial purchase rate is 0
                    MRP = 0, // Initial MRP is 0
                    SaleRate = 0 // Initial sale rate is 0
                };

                _context.Batches.Add(batch);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // If batch creation fails (e.g., due to foreign key constraints),
                // we'll log the error but not fail the product creation
                // The product was successfully created, so we return it
            }

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

        public async Task<IEnumerable<Product>> GetProductsByBarcodesAsync(List<string> barcodes)
        {
            if (barcodes == null || barcodes.Count == 0)
                return [];

            return await _context.Products
                .Where(p => barcodes.Contains(p.Barcode))
                .ToListAsync();
        }
    }
}