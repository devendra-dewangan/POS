using POS.Models;
using POS.Repos;

namespace POS.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(IUnitOfWork uow, ILogger<SupplierService> logger)
        {
            _unitOfWork = uow;
            _logger = logger;
        }

        public async Task<Supplier> AddSupplierAsync(string supplierName)
        {
            var supplier = new Supplier
            {
                Name = supplierName
            };

            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.CommitAsync();
            return supplier;
        }

        public async Task<Supplier> GetOrCreateSupplierAsync(string supplierName)
        {
            // Try to find existing supplier by name
            var supplier = await GetSupplierByNameAsync(supplierName);

            if (supplier != null)
            {
                return supplier;
            }

            // Create new supplier
            supplier = new Supplier
            {
                Name = supplierName
            };

            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.CommitAsync();
            return supplier;
        }

        public async Task<Supplier?> GetSupplierByNameAsync(string supplierName)
        {
            var supplier = await _unitOfWork.Suppliers.GetByNameAsync(supplierName);

            return supplier?.FirstOrDefault();
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _unitOfWork.Suppliers.GetAllAsync()?? [];
        }

        public async Task<IEnumerable<Supplier>> GetSuppliersByNamesAsync(List<string> supplierNames)
        {
            return await _unitOfWork.Suppliers.GetByNamesAsync(supplierNames);
        }

        public async Task<bool> BulkAddSuppliersAsync(List<Supplier> suppliers)
        {
            try
            {
                if (suppliers == null || suppliers.Count == 0)
                    return false;

                
                await _unitOfWork.Suppliers.AddBulkAsync(suppliers);

                // await _context.BulkInsertAsync(suppliers, new BulkConfig
                // {
                //     PreserveInsertOrder = true,
                //     SetOutputIdentity = true
                // });

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                _logger.LogError($"Error during bulk insert: {ex.Message}");
                return false;
            }
        }   
    }
}