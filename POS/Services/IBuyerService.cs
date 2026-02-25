using POS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.Services
{
    public interface IBuyerService
    {
        Task<Buyer> AddBuyerAsync(string name);
        Task<Buyer> GetOrCreateBuyerAsync(string name);
        Task<Buyer?> GetBuyerByNameAsync(string name);
        Task<IEnumerable<Buyer>> GetAllBuyersAsync();
    }
}