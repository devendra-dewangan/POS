using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Batch
    {
        public int Id { get; set; }
        
        [Required]
        public int PurchaseId { get; set; }
        
        public Purchase? Purchase { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative value")]
        public int Stock { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase Rate must be a positive value")]
        [Display(Name = "Purchase Rate")]
        public decimal PurchaseRate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MRP must be a positive value")]
        [Display(Name = "MRP")]
        public decimal MRP { get; set; }
    }
}
