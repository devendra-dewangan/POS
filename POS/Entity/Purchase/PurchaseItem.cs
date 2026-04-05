using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Entity
{
    public class PurchaseItem
    {
        public int Id { get; set; }
        
        [Required]
        public int PurchaseId { get; set; }
        
        public Purchase? Purchase { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public Product? Product { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be a non-negative value")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase Rate must be a non-negative value")]
        [Display(Name = "Purchase Rate")]
        public decimal PurchaseRate { get; set; }

        // Navigation
        [JsonIgnore]
        public ICollection<Batch> Batches { get; set; } = [];
    }
}