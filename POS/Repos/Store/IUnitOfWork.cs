using Microsoft.EntityFrameworkCore.Storage;

namespace POS.Repos
{
    public interface IUnitOfWork
    {
        IProductRepo Products {get;}
        IBatchRepo Batches {get;}
        ISaleItemRepo SaleItems {get;}
        ISaleRepo Sales {get;}
        IBuyerRepo Buyers {get;}
        ISupplierRepo Suppliers {get;}
        IPurchaseRepo Purchases {get;}
        IRefreshTokenRepo RefreshTokens { get; }
        IPurchaseItemRepo PurchaseItems { get; }
        IImportInfoRepo ImportInfos { get; }

        Task<int> CommitAsync(CancellationToken cancellationToke = default);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}