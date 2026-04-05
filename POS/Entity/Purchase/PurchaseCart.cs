namespace POS.Entity;

public enum CartStatus
{
    Open,
    Completed,
    Cancelled,
    Hold,
}

public class PurchaseCart
{
    public int Id { get; set; }

    public CartStatus Status { get; set; } = CartStatus.Open;

    public Purchase? Purchase { get; set; }
    public List<Batch> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}