using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Batch
    {
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public Product? Product { get; set; }
        
        public int? PurchaseId { get; set; }
        
        public Purchase? Purchase { get; set; }
        
        [Required]
        public decimal Stock { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase Stock must be a non-negative value")]
        [Display(Name = "Purchase Stock")]
        public decimal PurchaseStock { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase Rate must be a non-negative value")]
        [Display(Name = "Purchase Rate")]
        public decimal PurchaseRate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MRP must be a non-negative value")]
        [Display(Name = "MRP")]
        public decimal MRP { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Sale Rate must be a non-negative value")]
        [Display(Name = "Sale Rate")]
        public decimal SaleRate { get; set; }
    }
}
