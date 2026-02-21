using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    public class InstructorInfoUpdateRequestCreateDto
    {
        [Required]
        public long InstructorId { get; set; }

        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }

    public class InstructorInfoUpdateRequestReviewDto
    {
        [Required]
        public bool Approve { get; set; }

        public string? DenialReason { get; set; }
    }

    public class InstructorInfoUpdateRequestDto
    {
        public long Id { get; set; }
        public long InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string? SubmittedByUserId { get; set; }
        public InstructorInfoUpdateRequestStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }

        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? DenialReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
