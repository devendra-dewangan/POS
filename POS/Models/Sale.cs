using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class Sale
    {
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public Product? Product { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public DateTime SaleDate { get; set; }
    }
}