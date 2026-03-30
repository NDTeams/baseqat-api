using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Baseqat.EF.Models.Auth
{
    [Table("RevokedTokens")]
    public class RevokedToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Token { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        public DateTime RevokedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [MaxLength(50)]
        public string? RevokedFrom { get; set; } // IP Address or Device

        public virtual ApplicationUser User { get; set; }
    }
}
