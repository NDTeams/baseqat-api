using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public CourseLevel Level { get; set; }
        public string? Language { get; set; }
        public bool HasCertificate { get; set; }
        public decimal? Price { get; set; }
        public CourseDays CourseDays { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationInDays { get; set; }
        public int TotalDurationInHours { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CourseStatus Status { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        public long CourseCategoryId { get; set; }
        [Required]
        public long InstructorId { get; set; }
    }
    #endregion

    #region Update DTO
    public class CourseUpdateDto
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public CourseLevel? Level { get; set; }
        public string? Language { get; set; }
        public bool? HasCertificate { get; set; }
        public decimal? Price { get; set; }
        public CourseDays? CourseDays { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? DurationInDays { get; set; }
        public int? TotalDurationInHours { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CourseStatus? Status { get; set; }
        public bool? IsActive { get; set; }
        public long? CourseCategoryId { get; set; }
        public long? InstructorId { get; set; }
    }
    #endregion

    #region Response DTO
    public class CourseDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PromoVideoUrl { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string? Language { get; set; }
        public bool HasCertificate { get; set; }
        public decimal? Price { get; set; }
        public CourseDays CourseDays { get; set; }
        public string CourseDaysName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationInDays { get; set; }
        public int TotalDurationInHours { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public CourseStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public long CourseCategoryId { get; set; }
        public string CourseCategoryName { get; set; } = string.Empty;
        public long InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
    }
    #endregion

    #region Detail DTO (with related lists)
    public class CourseDetailDto : CourseDto
    {
        public List<CourseSectionDto> Sections { get; set; } = new();
        public List<CourseInstructorDto> Instructors { get; set; } = new();
    }
    #endregion

    #region Filter DTO
    public class CourseFilterDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public CourseLevel? Level { get; set; }
        public CourseStatus? Status { get; set; }
        public long? CourseCategoryId { get; set; }
        public long? InstructorId { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
    #endregion
}
