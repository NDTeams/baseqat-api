using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class StudentReviewCreateDto
    {
        [Required]
        public long InstructorId { get; set; }
        [Required]
        public long CourseId { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        [Range(1.0, 5.0)]
        public double Rating { get; set; }
        public string? Comment { get; set; }
    }
    #endregion

    #region Update DTO
    public class StudentReviewUpdateDto
    {
        [Range(1.0, 5.0)]
        public double? Rating { get; set; }
        public string? Comment { get; set; }
    }
    #endregion

    #region Response DTO
    public class StudentReviewDto
    {
        public long Id { get; set; }
        public long InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public long CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
    #endregion

    #region Filter DTO
    public class StudentReviewFilterDto
    {
        public long? Id { get; set; }
        public long? InstructorId { get; set; }
        public long? CourseId { get; set; }
        public string? UserId { get; set; }
        public double? MinRating { get; set; }
        public double? MaxRating { get; set; }
    }
    #endregion
}
