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

        Task<int> CommitAsync(CancellationToken cancellationToke = default);
    }
}