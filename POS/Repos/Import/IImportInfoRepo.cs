using POS.Entity;

namespace POS.Repos
{
    public interface IImportInfoRepo 
    {
        Task<IEnumerable<ImportInfo>?> GetByIdAsync(int id);
        Task AddAsync(ImportInfo importInfo);
    }
}