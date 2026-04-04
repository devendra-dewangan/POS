using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace POS.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public int? StoreId { get; set; }
        
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
