using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Models
{
    public class Product
    {
        public int Id { get; set; }
        
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
        
        [JsonIgnore]
        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }
}
