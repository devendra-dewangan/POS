using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string TokenHash  { get; set; } = string.Empty;
        public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7);
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string CreatedByIp { get; set; } = string.Empty;
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        
        [Required]
        public string UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}