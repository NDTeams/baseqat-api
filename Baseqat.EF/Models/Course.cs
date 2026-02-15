using Baseqat.EF.Models.Enums;

namespace Baseqat.EF.Models
{
    public class Course : AuditableEntity
    {
        public long Id { get; set; }

        // Basic info
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        // Media
        public string? ThumbnailUrl { get; set; } = string.Empty;
        public string? PromoVideoUrl { get; set; } = string.Empty;

        // Metadata
        public CourseLevel Level { get; set; }
        public string? Language { get; set; } 
        public bool HasCertificate { get; set; }

        // Pricing
        public decimal? Price { get; set; }


        // Duration
        public CourseDays CourseDays { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationInDays { get; set; }
        public int TotalDurationInHours { get; set; }

        // Dates 
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        
       

        // Status
        public CourseStatus Status { get; set; }

        public bool IsActive { get; set; } = true;

        // Relationships
        public long CourseCategoryId { get; set; }
        public CourseCategory CourseCategory { get; set; } = null!;

        public long InstructorId { get; set; }
        public Instructor Instructor { get; set; } = null!;

        public ICollection<CourseSection> Sections { get; set; } = new List<CourseSection>();
    }

}
