using System.Threading.Tasks;

namespace POS.Services
{
    public interface IImportService
    {
        Task<bool> ImportPurchaseDataAsync(string filePath);
        Task<bool> ImportSaleDataAsync(string filePath);
    }
}