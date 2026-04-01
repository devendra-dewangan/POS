
namespace POS.Repos
{
    public interface IRepository<T>
    {
        Task AddAsync(T value);
        Task<T?> GetByIDAsync(int id);
        Task UpdateAsync(T value);
        Task DeleteAsync(T value);
        Task AddBulkAsync(IEnumerable<T> values);
        Task<IEnumerable<T>?> GetAllAsync();
    }
}