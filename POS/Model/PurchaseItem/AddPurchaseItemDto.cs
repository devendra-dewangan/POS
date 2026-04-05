namespace POS.Model;

public record AddPurchaseItemRequestDto
(
    int ProductId,
    decimal UnitPrice,
    decimal Quantity,
    IEnumerable<AddBatchRequestDto>? Batches
);