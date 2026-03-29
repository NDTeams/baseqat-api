using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseEnrollmentCreateDto
    {
        [Required]
        public long CourseId { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
    }
    #endregion

    #region Response DTO
    public class CourseEnrollmentDto
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string EnrolledAt { get; set; } = string.Empty;
    }
    #endregion

    #region My Enrollment DTO (Client Dashboard)
    public class MyEnrollmentDto
    {
        public long EnrollmentId { get; set; }
        public string EnrolledAt { get; set; } = string.Empty;

        // Course details
        public long CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseSubtitle { get; set; }
        public string? ThumbnailUrl { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public CourseType CourseType { get; set; }
        public string CourseTypeName { get; set; } = string.Empty;
        public CourseStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int TotalDurationInHours { get; set; }
        public bool HasCertificate { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Location { get; set; }

        // Instructor
        public string InstructorName { get; set; } = string.Empty;
        public string? InstructorAvatarUrl { get; set; }

        // Stats
        public int TotalSections { get; set; }
        public int TotalEnrollments { get; set; }
    }
    #endregion

    #region Filter DTO
    public class CourseEnrollmentFilterDto
    {
        public long? Id { get; set; }
        public long? CourseId { get; set; }
        public string? UserId { get; set; }
        public DateTime? EnrolledFrom { get; set; }
        public DateTime? EnrolledTo { get; set; }
    }
    #endregion
}
