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
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Stock must be a non-negative value")]
        public decimal Stock { get; set; }
        
        [JsonIgnore]
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        
        [JsonIgnore]
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        
        [JsonIgnore]
        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }
}
