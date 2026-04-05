using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Entity
{
    public class SaleItem
    {
        public int Id { get; set; }
        
        [Required]
        public int SaleId { get; set; }
        
        public Sale? Sale { get; set; }
        
        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Sale Rate must be a positive value")]
        public decimal SaleRate { get; set; }
        
        [JsonIgnore]
        public ICollection<SaleBatch> SaleBatches { get; set; } = [];
    }
}