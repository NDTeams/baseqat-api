using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseLessonCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public LessonType LessonType { get; set; }
        public int DurationInMinutes { get; set; }
        public bool IsPreview { get; set; }
        public int Order { get; set; }
        [Required]
        public long CourseSectionId { get; set; }
    }
    #endregion

    #region Update DTO
    public class CourseLessonUpdateDto
    {
        public string? Title { get; set; }
        public LessonType? LessonType { get; set; }
        public int? DurationInMinutes { get; set; }
        public bool? IsPreview { get; set; }
        public int? Order { get; set; }
    }
    #endregion

    #region Response DTO
    public class CourseLessonDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public LessonType LessonType { get; set; }
        public string LessonTypeName { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public bool IsPreview { get; set; }
        public int Order { get; set; }
        public long CourseSectionId { get; set; }
    }
    #endregion

    #region Filter DTO
    public class CourseLessonFilterDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public LessonType? LessonType { get; set; }
        public long? CourseSectionId { get; set; }
        public bool? IsPreview { get; set; }
    }
    #endregion
}
