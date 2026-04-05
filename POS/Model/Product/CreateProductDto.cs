namespace POS.Model;

public record CreateProductRequestDto
(
    string ProductName,
    string Barcode,
    decimal MRP
);

public record CreateProductResponseDto
(
    int ProductId
);
