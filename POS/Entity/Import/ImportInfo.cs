using System.Text.Json.Serialization;
namespace POS.Entity;

public class ImportInfo
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.NotStarted;
    public ImportType ImportType { get; set; }
    public DateTime ImportDate { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<ImportPurchaseTemp> ImportPurchaseTemps { get; set; } = [];
}

public enum ImportStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public enum ImportType
{
    Purchase,
    Sale,
    Seller,
    Buyer,
    Product
}