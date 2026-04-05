using POS.Models;

namespace POS.Services
{
    public interface ISaleService
    {
        Task<int> AddSaleAsync(int buyerId);
        Task<IEnumerable<Sale>?> GetSaleByInvoiceAsync(string invoiceNumber);
        Task<IEnumerable<Sale>?> GetAllSalesAsync();
        Task<bool> AddSaleBulkAsync(IEnumerable<Sale> sales);
    }
}