

using POS.Entity;

namespace POS.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, ICollection<string> roles);
        string GenerateRefreshToken();
        string HashToken(string token);
    }
}