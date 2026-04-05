using POS.Models;

namespace POS.Repos
{
    public interface IProductRepo : IRepository<Product>, IAddBulk<Product>
    {
        Task<Product?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<Product>?> GetByNameAsync(string name);
        Task<IEnumerable<Product>?> GetProductsByBarcodesAsync(IEnumerable<string> barcodes);
    }
}