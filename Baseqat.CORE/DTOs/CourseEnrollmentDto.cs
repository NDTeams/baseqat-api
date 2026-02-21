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
