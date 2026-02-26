using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace POS.Services
{
    public interface IImportService
    {
        Task<bool> ImportPurchaseDataAsync(IFormFile file);
        Task<bool> ImportSaleDataAsync(IFormFile file);
    }
}
