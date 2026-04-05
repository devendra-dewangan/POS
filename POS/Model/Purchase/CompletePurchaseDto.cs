namespace POS.Model;

public record CompletePurchaseResponse
(
    string InvoiceNumber,
    DateTime PurchaseDate,
    decimal TotalAmount,
    List<PurchaseItemDto> Items
);

public record PurchaseItemDto
(
    int ProductId,
    string ProductCode,
    string ProductName,
    decimal UnitPrice,
    decimal Quantity,
    decimal TotalPrice
);