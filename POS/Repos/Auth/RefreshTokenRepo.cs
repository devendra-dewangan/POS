using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Entity;

namespace POS.Repos
{
    public class RefreshTokenRepo(AppDbContext context) : IRefreshTokenRepo
    {
        private readonly AppDbContext _context = context;

        public async Task AddAsync(RefreshToken value)
        {
            await _context.RefreshTokens.AddAsync(value);
        }

        public async Task DeleteAsync(RefreshToken value)
        {
            _context.RefreshTokens.Remove(value);
        }

        public async Task<IEnumerable<RefreshToken>?> GetAllAsync()
        {
            return await _context.RefreshTokens.ToListAsync();
        }

        public async Task<RefreshToken?> GetByIDAsync(int id)
        {
            return await _context.RefreshTokens.FindAsync(id);
        }

        public async Task UpdateAsync(RefreshToken value)
        {
            _context.RefreshTokens.Update(value);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == token);
        }
    }
}
        