using System.ComponentModel.DataAnnotations;

namespace Baseqat.EF.Models
{
    public class ConsultationCategory : DeleteEntity
    {
        public long Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Many-to-many with Consultant
        public ICollection<ConsultantConsultationCategory> ConsultantCategories { get; set; } = new List<ConsultantConsultationCategory>();
    }
}
