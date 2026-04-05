using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Entity;
using SQLitePCL;

namespace POS.Repos
{
    public class ImportInfoRepo : IImportInfoRepo
    {
        private AppDbContext _context;
        public ImportInfoRepo(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddAsync(ImportInfo importInfo)
        {
            await _context.ImportInfos.AddAsync(importInfo);
        }

        public async Task<IEnumerable<ImportInfo>?> GetByIdAsync(int id)
        {
            return await _context.ImportInfos.Where(i => i.Id == id).ToListAsync();
        }
    }
}
