using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Models
{
    public class Sale
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime InvoiceDate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a non-negative value")]
        public decimal TotalAmount { get; set; }
        
        [JsonIgnore]
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        
        [Required]
        public DateTime SaleDate { get; set; }
        
        [Required]
        public int BuyerId { get; set; }
        
        public Buyer? Buyer { get; set; }
    }
}
