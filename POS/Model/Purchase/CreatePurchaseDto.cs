namespace POS.Model;

public record CreatePurchaseRequest
{
    public int SupplierId;
}

public record CreatePurchaseResponse
{
    public int PurchaseDraftId;
}