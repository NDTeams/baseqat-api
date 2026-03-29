using Baseqat.EF.Models.Enums;

namespace Baseqat.EF.Models
{
    public class MediaCenter : AuditableEntity
    {
        public long Id { get; set; }

        // Basic Info
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
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

        // Status
        public bool IsActive { get; set; } = true;
    }
}
