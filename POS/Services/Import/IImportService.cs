namespace POS.Services
{
    public interface IImportService
    {
        Task<bool> ImportPurchaseDataAsync(string file);
        Task<bool> ImportSaleDataAsync(string file);
        Task<bool> DeleteImportDataAsync(int importId);
    }
}
