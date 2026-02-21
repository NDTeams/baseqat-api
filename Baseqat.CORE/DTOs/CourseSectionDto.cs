using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseSectionCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public long CourseId { get; set; }
    }
    #endregion

    #region Update DTO
    public class CourseSectionUpdateDto
    {
        public string? Title { get; set; }
        public int? Order { get; set; }
    }
    #endregion

    #region Response DTO
    public class CourseSectionDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<CourseLessonDto> Lessons { get; set; } = new();
    }
    #endregion

    #region Filter DTO
    public class CourseSectionFilterDto
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public long? CourseId { get; set; }
    }
    #endregion
}
