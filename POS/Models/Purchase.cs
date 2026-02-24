using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        
        [Required]
        public int SupplierId { get; set; }
        
        public Supplier? Supplier { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime PurchaseDate { get; set; }
        
        [JsonIgnore]
        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }
}
