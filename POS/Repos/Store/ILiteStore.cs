using LiteDB;
using POS.Entity;

namespace POS.Repos;
public interface ILiteStore
{
    ILiteCollection<PurchaseCart> PurchaseCarts {get;}
    ILiteCollection<SaleCart> SaleCarts {get;}
    ILiteCollection<ImportCart> ImportInfos { get; }
}