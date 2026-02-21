using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class Instructor : DeleteEntity
    {
        public long Id { get; set; }

        // ربط بحساب المستخدم
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Basic info
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // مثل "خبيرة منتج وتجربة مستخدم"
        public string Bio { get; set; } = string.Empty; // نبذة عن المدرب
        public string AvatarUrl { get; set; } = string.Empty;
        public string? CvUrl { get; set; } // السيرة الذاتية
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? XUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }

        // Info update request workflow (stored in same table)
        public InstructorInfoUpdateRequestStatus? InfoUpdateRequestStatus { get; set; }
        public string? InfoUpdateSubmittedByUserId { get; set; }
        public string? InfoUpdateReviewedByUserId { get; set; }
        public DateTime? InfoUpdateReviewedAt { get; set; }
        public string? InfoUpdateDenialReason { get; set; }
        public string? InfoUpdatePayloadJson { get; set; }

        public Gender Gender { get; set; }
        public bool IsActive { get; set; } = true; // حالة التفعيل

        // Stats (يمكن حسابها من الدورات)
        public double? Rating { get; set; } // متوسط تقييم الطلاب
        public int? TotalStudents { get; set; }
        public int? TotalCources { get; set; }

        // المهارات
        public ICollection<InstructorSkill> Skills { get; set; } = new List<InstructorSkill>();

        // الدورات التي يدرّسها
        public ICollection<CourseInstructor> CourseInstructors { get; set; } = new List<CourseInstructor>();

        // آراء الطلاب
        public ICollection<StudentReview> StudentReviews { get; set; } = new List<StudentReview>();
    }


}
