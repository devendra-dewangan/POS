using POS.Entity;

namespace POS.Repos
{
    public interface IProductRepo : IRepository<Product>, IAddBulk<Product>
    {
        Task<IEnumerable<Product>?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<Product>?> GetByNameAsync(string name);
        Task<IEnumerable<Product>?> GetByBarcodesAsync(IEnumerable<string> barcodes);
        Task<IEnumerable<Product>?> GetByIdsAsync(IEnumerable<int> ids);
    }
}