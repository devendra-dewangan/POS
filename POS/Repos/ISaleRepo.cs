using POS.Models;

namespace POS.Repos
{
    public interface ISaleRepo : IRepository<Sale>, IAddBulk<Sale>
    {
        Task<IEnumerable<Sale>?> GetByInvoiceNumberAsync(string invoiceNumber);
    }
}