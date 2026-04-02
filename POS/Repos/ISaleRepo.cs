using POS.Models;

namespace POS.Repos
{
    public interface ISaleRepo : IRepository<Sale>
    {
        Task<IEnumerable<Sale>?> GetByInvoiceNumberAsync(string invoiceNumber);
    }
}