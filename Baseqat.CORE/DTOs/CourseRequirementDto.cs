using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseRequirementCreateDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        [Required]
        public long CourseId { get; set; }
    }
    #endregion

    #region Update DTO
    public class CourseRequirementUpdateDto
    {
        public string? Text { get; set; }
        public int? Order { get; set; }
    }
    #endregion

    #region Response DTO
    public class CourseRequirementDto
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public long CourseId { get; set; }
    }
    #endregion

    #region Filter DTO
    public class CourseRequirementFilterDto
    {
        public long? Id { get; set; }
        public string? Text { get; set; }
        public long? CourseId { get; set; }
    }
    #endregion
}
