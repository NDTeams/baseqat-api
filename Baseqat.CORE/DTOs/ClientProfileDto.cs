using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Response DTO
    public class ClientProfileDto
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        // من ApplicationUser
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime JoinedDate { get; set; }

        // من ClientProfile
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public string? CvUrl { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<string> Interests { get; set; } = new();

        // روابط اجتماعية
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? WebsiteUrl { get; set; }

        // اكتمال الملف
        public int ProfileCompletion { get; set; }
        public ProfileCompletionDetails CompletionDetails { get; set; } = new();
    }

    public class ProfileCompletionDetails
    {
        public bool HasFullName { get; set; }
        public bool HasProfilePicture { get; set; }
        public bool HasPhoneNumber { get; set; }
        public bool HasDateOfBirth { get; set; }
        public bool HasBio { get; set; }
        public bool HasAddress { get; set; }
        public bool HasGender { get; set; }
        public bool HasCv { get; set; }
    }
    #endregion

    #region Update DTO
    public class UpdateClientProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public Gender? Gender { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Interests { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? WebsiteUrl { get; set; }
    }
    #endregion

    #region Change Password DTO
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
    #endregion
}
