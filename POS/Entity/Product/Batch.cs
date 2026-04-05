using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace POS.Entity
{
    public class Batch
    {
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public Product? Product { get; set; }
        
        public int? PurchaseItemId { get; set; }
        
        public PurchaseItem? PurchaseItem { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Remaining Stock must be a non-negative value")]
        [Display(Name = "Remaining Stock")]
        public decimal RemainingStock { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Opening Stock must be a non-negative value")]
        [Display(Name = "Opening Stock")]
        public decimal OpeningStock { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MRP must be a non-negative value")]
        [Display(Name = "MRP")]
        public decimal MRP { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Sale Rate must be a non-negative value")]
        [Display(Name = "Sale Rate")]
        public decimal SaleRate { get; set; }

        [JsonIgnore]
        public ICollection<SaleBatch> SaleBatches { get; set; } = [];
    }
}
