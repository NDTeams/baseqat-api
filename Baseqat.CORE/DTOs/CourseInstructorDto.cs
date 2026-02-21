using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseInstructorCreateDto
    {
        [Required]
        public long CourseId { get; set; }
        [Required]
        public long InstructorId { get; set; }
    }
    #endregion

    #region Response DTO
    public class CourseInstructorDto
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public long InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string InstructorTitle { get; set; } = string.Empty;
        public string? InstructorAvatarUrl { get; set; }
    }
    #endregion

    #region Filter DTO
    public class CourseInstructorFilterDto
    {
        public long? CourseId { get; set; }
        public long? InstructorId { get; set; }
    }
    #endregion
}
