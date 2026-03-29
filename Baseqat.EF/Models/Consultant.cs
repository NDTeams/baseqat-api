using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using System;
using System.Collections.Generic;

namespace Baseqat.EF.Models
{
    public class Consultant : DeleteEntity
    {
        public long Id { get; set; }

        // ربط بحساب المستخدم
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Basic info
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // مثل "مستشار إداري"
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string? CvUrl { get; set; }
        public int? YearsOfExperience { get; set; }

        // Social Links
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }

        // Consultant-specific
        public string? Specialty { get; set; } // التخصص (استشارات إدارية، تقنية، مالية)
        public decimal? HourlyRate { get; set; } // سعر الساعة
        public string? Availability { get; set; } // أوقات التوفر

        // Request workflow
        public InstructorRequestStatus? RequestStatus { get; set; }
        public string? SubmittedByUserId { get; set; }
        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? DenialReason { get; set; }
        public string? PayloadJson { get; set; }

        public Gender Gender { get; set; }
        public bool IsActive { get; set; } = true;

        // Stats
        public double? Rating { get; set; }

        // المهارات
        public ICollection<ConsultantSkill> Skills { get; set; } = new List<ConsultantSkill>();

        // أقسام الاستشارات
        public ICollection<ConsultantConsultationCategory> ConsultantCategories { get; set; } = new List<ConsultantConsultationCategory>();

        // طلبات الاستشارة
        public ICollection<ConsultationRequest> ConsultationRequests { get; set; } = new List<ConsultationRequest>();
    }
}
