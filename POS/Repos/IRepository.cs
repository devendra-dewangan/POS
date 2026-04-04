
namespace POS.Repos
{
    public interface IAddBulk<T>
    {
        Task AddBulkAsync(IEnumerable<T> values);
    }


    public interface IRepository<T> 
    {
        Task AddAsync(T value);
        Task<T?> GetByIDAsync(int id);
        Task UpdateAsync(T value);
        Task DeleteAsync(T value);
        Task<IEnumerable<T>?> GetAllAsync();
    }
}