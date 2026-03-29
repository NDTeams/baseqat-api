using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    public class ConsultationCategoryCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ConsultationCategoryUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ConsultationCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ConsultantCount { get; set; }
    }

    public class ConsultationCategoryFilterDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
