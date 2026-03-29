using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class ConsultantCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public Gender Gender { get; set; }
        public string? UserId { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Specialty { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Availability { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
    #endregion

    #region Create With Account DTO
    public class ConsultantCreateWithAccountDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public Gender Gender { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Specialty { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Availability { get; set; }
        public List<long>? CategoryIds { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }

        // بيانات الحساب
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
    #endregion

    #region Update DTO
    public class ConsultantUpdateDto
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public double? Rating { get; set; }
        public string? UserId { get; set; }
        public bool? IsActive { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Specialty { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Availability { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
    #endregion

    #region Response DTO
    public class ConsultantDto
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhoneNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CvUrl { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public bool IsActive { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Specialty { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Availability { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }

        public string? SubmittedByUserId { get; set; }
        public InstructorRequestStatus? RequestStatus { get; set; }
        public string? RequestStatusName { get; set; }
        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? DenialReason { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    #endregion

    #region Detail DTO
    public class ConsultantDetailDto : ConsultantDto
    {
        public List<ConsultantSkillDto> Skills { get; set; } = new();
        public List<ConsultationCategoryDto> Categories { get; set; } = new();
    }
    #endregion

    #region Filter DTO
    public class ConsultantFilterDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Specialty { get; set; }
        public Gender? Gender { get; set; }
        public bool? IsActive { get; set; }
    }
    #endregion

    #region Request Create DTO
    public class ConsultantRequestCreateDto
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Specialty { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Availability { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public List<string>? Skills { get; set; }
        public List<long>? CategoryIds { get; set; }
    }

    public class ConsultantRequestReviewDto
    {
        [Required]
        public bool Approve { get; set; }
        public string? DenialReason { get; set; }
    }
    #endregion

    #region Skill DTOs
    public class ConsultantSkillCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public long? ConsultantId { get; set; }
    }

    public class ConsultantSkillUpdateDto
    {
        public string? Name { get; set; }
    }

    public class ConsultantSkillDto
    {
        public long Id { get; set; }
        public long? ConsultantId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ConsultantSkillFilterDto
    {
        public long? Id { get; set; }
        public long? ConsultantId { get; set; }
        public string? Name { get; set; }
    }
    #endregion
}
