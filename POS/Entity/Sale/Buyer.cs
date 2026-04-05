using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Entity
{
    public class Buyer
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [JsonIgnore]
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
