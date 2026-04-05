using POS.Models;

namespace POS.Repos
{
    public interface IRefreshTokenRepo : IRepository<RefreshToken>
    {
            Task<RefreshToken?> GetByTokenAsync(string token);
    }
}