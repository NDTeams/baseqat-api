using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    public class MediaCenterCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public MediaType MediaType { get; set; }
        public string? Category { get; set; }

        // Article-specific
        public string? Author { get; set; }
        public string? Content { get; set; }
        public int? ReadingTimeMinutes { get; set; }

        // Event-specific
        public DateTime? EventDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public string? EventTime { get; set; }
        public string? EventLocation { get; set; }

        // Media
        public string? VideoUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class MediaCenterUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public MediaType? MediaType { get; set; }
        public string? Category { get; set; }

        // Article-specific
        public string? Author { get; set; }
        public string? Content { get; set; }
        public int? ReadingTimeMinutes { get; set; }

        // Event-specific
        public DateTime? EventDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public string? EventTime { get; set; }
        public string? EventLocation { get; set; }

        // Media
        public string? VideoUrl { get; set; }

        public bool? IsActive { get; set; }
    }

    public class MediaCenterDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public MediaType MediaType { get; set; }
        public string MediaTypeName { get; set; } = string.Empty;
        public string? Category { get; set; }

        // Article-specific
        public string? Author { get; set; }
        public string? Content { get; set; }
        public int? ReadingTimeMinutes { get; set; }

        // Event-specific
        public DateTime? EventDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public string? EventTime { get; set; }
        public string? EventLocation { get; set; }

        // Media
        public string? VideoUrl { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MediaCenterFilterDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public MediaType? MediaType { get; set; }
        public string? Category { get; set; }
        public bool? IsActive { get; set; }
    }
}
