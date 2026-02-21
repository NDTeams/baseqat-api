using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class InstructorCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public Gender Gender { get; set; }
        public string? UserId { get; set; } // ربط بحساب موجود (اختياري)
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
    #endregion

    #region Create With Account DTO
    public class InstructorCreateWithAccountDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public Gender Gender { get; set; }
        public int? YearsOfExperience { get; set; }
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
    public class InstructorUpdateDto
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public double? Rating { get; set; }
        public int? TotalStudents { get; set; }
        public int? TotalCources { get; set; }
        public string? UserId { get; set; } // ربط/إلغاء ربط بحساب
        public bool? IsActive { get; set; } // حالة التفعيل
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
    #endregion

    #region Response DTO
    public class InstructorDto
    {
        public long Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CvUrl { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public int? TotalStudents { get; set; }
        public int? TotalCources { get; set; }
        public bool IsActive { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
    #endregion

    #region Detail DTO
    public class InstructorDetailDto : InstructorDto
    {
        public List<InstructorSkillDto> Skills { get; set; } = new();
        public List<CourseDto> Courses { get; set; } = new();
        public List<StudentReviewDto> Reviews { get; set; } = new();
    }
    #endregion

    #region Filter DTO
    public class InstructorFilterDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public Gender? Gender { get; set; }
        public double? MinRating { get; set; }
        public bool? IsActive { get; set; }
    }
    #endregion
}
