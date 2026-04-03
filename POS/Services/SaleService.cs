using POS.Models;
using POS.Repos;
namespace POS.Services
{
    public class SaleService : ISaleService
    {
        private IUnitOfWork _unitOfWork;
        private ILiteStore _liteStore;

        public SaleService(IUnitOfWork uow, ILiteStore liteStore)
        {
            _unitOfWork = uow;
            _liteStore = liteStore;
        }

        public async Task<int> AddSaleAsync(int buyerId)
        {

            var sale = new SaleCart
            {
                Sale = new Sale
                {
                    BuyerId = buyerId
                }
            };
            _liteStore.SaleCarts.Upsert(sale);
            return sale.Id;
        }

        public async Task<Sale> CompleteSaleAsync(int saleCartId)
        {
            var saleCart = _liteStore.SaleCarts.FindById(saleCartId);
            if (saleCart == null || saleCart.Status == CartStatus.Completed)
                throw new InvalidOperationException("Invalid sale cart.");

            if (saleCart.Items.Count == 0)
                throw new InvalidOperationException("Sale cart is empty.");

            // Here you would typically save the purchase to the main database
            var sale = saleCart.Sale!;
            await _unitOfWork.Sales.AddAsync(sale);
            await _unitOfWork.CommitAsync();

            var saleItems = saleCart.Items;

            saleItems.ForEach(item => item.SaleId = sale.Id); // Update the cart with the saved purchase (with ID)
            await _unitOfWork.SaleItems.AddBulkAsync(saleItems);
            await _unitOfWork.CommitAsync();

            // Update the cart status
            saleCart.Status = CartStatus.Completed;
            _liteStore.SaleCarts.Update(saleCart);
            _liteStore.SaleCarts.Delete(saleCartId);
            return sale;
        }

        public async Task<IEnumerable<Sale>?> GetSaleByInvoiceAsync(string invoiceNumber)
        {
            var sales = await _unitOfWork.Sales.GetByInvoiceNumberAsync(invoiceNumber);

            return sales;
        }

        public async Task<IEnumerable<Sale>?> GetAllSalesAsync()
        {
            return await _unitOfWork.Sales.GetAllAsync();
        }

        public async Task<bool> AddSaleItemAsync(int saleCartId, int batchId, decimal quantity)
        {
            var saleCart = _liteStore.SaleCarts.FindById(saleCartId);
            if (saleCart == null || saleCart.Status == CartStatus.Completed)
                throw new InvalidOperationException("Invalid sale cart.");

            var batch = await _unitOfWork.Batches.GetByIDAsync(batchId) 
                    ?? throw new InvalidOperationException("Insufficient stock in the batch.");

            var saleItem = new SaleItem
            {
                BatchId = batchId,
                Quantity = quantity,
                UnitPrice = batch.SaleRate,
            };

            saleCart.Items.Add(saleItem);
            _liteStore.SaleCarts.Update(saleCart);
            return true;
        }

        public async Task<bool> AddSaleBulkAsync(IEnumerable<Sale> sales)
        {
            try
            {
                await _unitOfWork.Sales.AddBulkAsync(sales);
                await _unitOfWork.CommitAsync();
                // await _context.BulkInsertAsync(sales, new BulkConfig
                // {
                //     PreserveInsertOrder = true,
                //     SetOutputIdentity = true
                // });
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                System.Console.WriteLine($"Error adding sales in bulk: {ex.Message}");
                return false;
            }
        }

    }
}