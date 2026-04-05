using POS.Entity;

namespace POS.Repos
{
    public interface IRefreshTokenRepo : IRepository<RefreshToken>
    {
            Task<RefreshToken?> GetByTokenAsync(string token);
    }
}