using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative value")]
        public int Stock { get; set; }
        
        [JsonIgnore]
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        
        [JsonIgnore]
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
