using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseReviewCreateDto
    {
        [Required]
        public long CourseId { get; set; }
        [Required]
        public long UserId { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
    #endregion

    #region Update DTO
    public class CourseReviewUpdateDto
    {
        [Range(1, 5)]
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }
    #endregion

    #region Response DTO
    public class CourseReviewDto
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
    #endregion

    #region Filter DTO
    public class CourseReviewFilterDto
    {
        public long? Id { get; set; }
        public long? CourseId { get; set; }
        public long? UserId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
    }
    #endregion
}
