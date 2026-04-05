namespace POS.Model;

public record AddBatchRequestDto
(
    int ProductId,
    string BatchNumber,
    decimal Quantity,
    decimal UnitPrice,
    decimal MRP
);

public record AddBatchResponseDto
(
    int PurchaseDraftId
);