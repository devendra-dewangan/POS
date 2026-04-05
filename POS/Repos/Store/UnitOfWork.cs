using Microsoft.EntityFrameworkCore.Storage;
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

        private IPurchaseItemRepo? _purchaseItems;
        public IPurchaseItemRepo PurchaseItems => _purchaseItems ??= new PurchaseItemRepo(context);

        private IImportInfoRepo? _importInfos;
        public IImportInfoRepo ImportInfos => _importInfos ??= new ImportInfoRepo(context);

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await context.Database.BeginTransactionAsync();
        }

    }
}

      