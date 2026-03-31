using POS.Models;

namespace POS.Services
{
    public interface ISaleService
    {
        int CompleteSale(IEnumerable<SaleItem> saleItems);
    }
}