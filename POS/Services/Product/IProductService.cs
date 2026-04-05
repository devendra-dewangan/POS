using POS.Entity;

namespace POS.Services
{
    public interface IProductService
    {
        Task<Product> AddProductAsync(string productName, string barcode , decimal MRP);
        Task<IEnumerable<Product>?> GetProductByNameAsync(string productName);
        Task<IEnumerable<Product>?> GetProductByBarcodeAsync(string barcode);
        Task<IEnumerable<Product>?> GetAllProductsAsync();
    }
}