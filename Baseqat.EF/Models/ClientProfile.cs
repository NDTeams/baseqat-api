using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.EF.Models
{
    public class ClientProfile : AuditableEntity
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // معلومات أساسية
        [MaxLength(500)]
        public string? Bio { get; set; }
        [MaxLength(300)]
        public string? Address { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;
        [MaxLength(500)]
        public string? CvUrl { get; set; }

        // مهارات واهتمامات (JSON arrays)
        public string? SkillsJson { get; set; }
        public string? InterestsJson { get; set; }

        // روابط اجتماعية
        [MaxLength(300)]
        public string? LinkedInUrl { get; set; }
        [MaxLength(300)]
        public string? XUrl { get; set; }
        [MaxLength(300)]
        public string? InstagramUrl { get; set; }
        [MaxLength(300)]
        public string? FacebookUrl { get; set; }
        [MaxLength(300)]
        public string? GitHubUrl { get; set; }
        [MaxLength(300)]
        public string? WebsiteUrl { get; set; }
    }
}
