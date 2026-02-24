using System.ComponentModel.DataAnnotations;

namespace POS.Models
{
    public class SaleItem
    {
        public int Id { get; set; }
        
        [Required]
        public int SaleId { get; set; }
        
        public Sale? Sale { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public Product? Product { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a positive value")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
    }
}