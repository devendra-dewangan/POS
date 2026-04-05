namespace POS.Entity;

public class ImportCart
{
    public int Id { get; set; }
    public ImportPurchaseTemp? Item { get; set; }
    public int ImportId { get; set; }
}