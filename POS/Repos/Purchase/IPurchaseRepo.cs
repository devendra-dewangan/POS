using POS.Models;

namespace POS.Repos
{
    public interface IPurchaseRepo : IRepository<Purchase>, IAddBulk<Purchase>
    {
        Task<IEnumerable<Purchase>?> GetByInvoiceNumbersAsync(IEnumerable<string> invoiceNumbers);
        Task<IEnumerable<Purchase>?> GetByInvoiceNumberAsync(string invoiceNumber);
    }
}