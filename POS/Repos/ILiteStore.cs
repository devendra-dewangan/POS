using LiteDB;
using POS.Models;

namespace POS.Repos;
public interface ILiteStore
{
    ILiteCollection<PurchaseCart> PurchaseCarts {get;}
    ILiteCollection<SaleCart> SaleCarts {get;}
}