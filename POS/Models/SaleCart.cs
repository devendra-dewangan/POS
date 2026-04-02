namespace POS.Models;

public class SaleCartItem
{
    public int BatchId { get; set; }
    public decimal Quantity { get; set; }
}

public class SaleCart
{
    public int Id { get; set; }
    public Buyer? Buyer { get; set; }
    public List<SaleCartItem> Items { get; set; } = new();
    public CartStatus Status { get; set; } = CartStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}