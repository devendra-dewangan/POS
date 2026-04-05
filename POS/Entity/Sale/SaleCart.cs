namespace POS.Models;

public class SaleCart
{
    public int Id { get; set; }
    public Sale? Sale { get; set; }
    public List<SaleItem> Items { get; set; } = new();
    public CartStatus Status { get; set; } = CartStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}