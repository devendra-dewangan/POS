using POS.Entity;

namespace POS.Services
{
    public interface ISupplierService
    {
        Task<Supplier> AddSupplierAsync(string supplierName);
        Task<Supplier> GetOrCreateSupplierAsync(string supplierName);
        Task<Supplier?> GetSupplierByNameAsync(string supplierName);
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<IEnumerable<Supplier>> GetSuppliersByNamesAsync(List<string> supplierNames);

        Task<bool> BulkAddSuppliersAsync(List<Supplier> suppliers);
    }
}