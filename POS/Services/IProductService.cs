using POS.Models;

namespace POS.Services
{
    public interface IProductService
    {
        Task<Product> AddProductAsync(string productName, string productCode, string barcode);
        Task<Product> GetOrCreateProductAsync(string productName, string productCode = "", string barcode = "");
        Task<Product?> GetProductByNameAsync(string productName);
        Task<Product?> GetProductByBarcodeAsync(string barcode);
        Task<IEnumerable<Product>> GetAllProductsAsync();
    }
}