using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class InstructorSkillCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public long? InstructorId { get; set; }
    }
    #endregion

    #region Update DTO
    public class InstructorSkillUpdateDto
    {
        public string? Name { get; set; }
    }
    #endregion

    #region Response DTO
    public class InstructorSkillDto
    {
        public long Id { get; set; }
        public long? InstructorId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    #endregion

    #region Filter DTO
    public class InstructorSkillFilterDto
    {
        public long? Id { get; set; }
        public long? InstructorId { get; set; }
        public string? Name { get; set; }
    }
    #endregion
}
