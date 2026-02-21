using Baseqat.EF.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class StudentReview
    {
        public long Id { get; set; }

        // العلاقة بالمدرب
        public long InstructorId { get; set; }
        public Instructor Instructor { get; set; } = null!;

        // العلاقة بالدورة
        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;


        // العلاقة بالمستخدم
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        // تقييم وتعليق
        public double Rating { get; set; } // 1.0 - 5.0
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        


    }

}
