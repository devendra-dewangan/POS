using System.ComponentModel.DataAnnotations;

namespace POS.Entity
{
    public class SaleBatch
    {
        public int Id { get; set; }
        
        [Required]
        public int SaleItemId { get; set; }
        public SaleItem? SaleItem { get; set; }

        [Required]
        public int BatchId { get; set; }
        public Batch? Batch { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity Taken must be a non-negative value")]
        [Display(Name = "Quantity Taken")]
        public decimal QuantityTaken { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase Price must be a positive value")]
        [Display(Name = "Purchase Price")]
        public decimal PurchasePrice { get; set; }
    }
}