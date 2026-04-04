using POS.Data;

namespace POS.Repos
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        private IProductRepo? _products;
        public IProductRepo Products => _products ??= new ProductRepo(context);

        private IBatchRepo? _batches;
        public IBatchRepo Batches => _batches ??= new BatchRepo(context);

        private ISaleItemRepo? _saleItems;
        public ISaleItemRepo SaleItems => _saleItems ??= new SaleItemRepo(context);

        private ISaleRepo? _sales;
        public ISaleRepo Sales => _sales ??= new SaleRepo(context);

        private IBuyerRepo? _buyers;
        public IBuyerRepo Buyers => _buyers ??= new BuyerRepo(context);

        private ISupplierRepo? _suppliers;
        public ISupplierRepo Suppliers => _suppliers ??= new SupplierRepo(context);

        private IPurchaseRepo? _purchases;
        public IPurchaseRepo Purchases => _purchases ??= new PurchaseRepo(context);

        private IRefreshTokenRepo? _refreshTokens;
        public IRefreshTokenRepo RefreshTokens => _refreshTokens ??= new RefreshTokenRepo(context);

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

    }
}

      