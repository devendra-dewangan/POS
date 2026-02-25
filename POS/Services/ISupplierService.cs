using POS.Models;

namespace POS.Services
{
    public interface ISupplierService
    {
        Task<Supplier> AddSupplierAsync(string supplierName);
        Task<Supplier> GetOrCreateSupplierAsync(string supplierName);
        Task<Supplier?> GetSupplierByNameAsync(string supplierName);
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
    }
}