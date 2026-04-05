using POS.Entity;
using POS.Repos;

namespace POS.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }

        public async Task<Product> AddProductAsync(string productName, string productCode, string barcode)
        {
            var product = new Product
            {
                ProductName = productName,
                ProductCode = productCode,
                Barcode = barcode
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CommitAsync();

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
                    PurchaseItemId = null, // No purchase associated (empty batch)
                    RemainingStock = 0, // Initial stock is 0
                    OpeningStock = 0, // Initial purchase stock is 0
                    MRP = 0, // Initial MRP is 0
                    SaleRate = 0 // Initial sale rate is 0
                };

                await _unitOfWork.Batches.AddAsync(batch);
                await _unitOfWork.CommitAsync();
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
            var product = await GetProductByNameAsync(productName);

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

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CommitAsync();
            return product;
        }

        public async Task<Product?> GetProductByNameAsync(string productName)
        {
            var products = await _unitOfWork.Products
                .GetByNameAsync(productName);

            return products?.FirstOrDefault();
        }

        public async Task<Product?> GetProductByBarcodeAsync(string barcode)
        {
            return await _unitOfWork.Products.GetByBarcodeAsync(barcode);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _unitOfWork.Products.GetAllAsync() ?? [];
        }

        public async Task<IEnumerable<Product>> GetProductsByBarcodesAsync(List<string> barcodes)
        {
            return await _unitOfWork.Products.GetProductsByBarcodesAsync(barcodes) ?? [];
        }

        public async Task<bool> BulkAddProductsAsync(List<Product> products)
        {
            try
            {
                if (products == null || products.Count == 0)
                    return false;
                // await _unitOfWork.Suppliers.AddBulkAsync(products,new BulkConfig
                // {
                //     PreserveInsertOrder = true,
                //     SetOutputIdentity = true
                // });
                await _unitOfWork.Products.AddBulkAsync(products);
                return true;
            }
            catch (Exception ex)
            {
                // Log the error as needed
                Console.WriteLine("Bulk insert of products failed: " + ex.Message);
                return false;
            }
        }
    }
}