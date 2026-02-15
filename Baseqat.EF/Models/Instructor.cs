using Baseqat.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class Instructor : AuditableEntity
    {
        public long Id { get; set; }

        // Basic info
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // مثل "خبيرة منتج وتجربة مستخدم"
        public string Bio { get; set; } = string.Empty; // نبذة عن المدرب
        public string AvatarUrl { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        // Stats (يمكن حسابها من الدورات)
        public double? Rating { get; set; } // متوسط تقييم الطلاب
        public int? TotalStudents { get; set; }
        public int? TotalCources { get; set; }

        // المهارات
        public ICollection<InstructorSkill> Skills { get; set; } = new List<InstructorSkill>();

        // الدورات التي يدرّسها
        public ICollection<CourseInstructor> CourseInstructors { get; set; } = new List<CourseInstructor>();

        // آراء الطلاب
        public ICollection<StudentReview> Reviews { get; set; } = new List<StudentReview>();
    }


}
