using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Entity
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; } = string.Empty;
        
        [StringLength(100)]
        [Display(Name = "Barcode")]
        public string Barcode { get; set; } = string.Empty;

        [Display(Name = "Total Stock")]
        public decimal TotalStock { get; set; } = 0;
        
        [JsonIgnore]
        public ICollection<Batch> Batches { get; set; } = [];

        [JsonIgnore]
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = [];
    }
}
