using Baseqat.EF.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;


namespace Baseqat.EF.Models
{
    public class CourseEnrollment
    {
        public long Id { get; set; }

        // العلاقة بالكورس
        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;

        // العلاقة بالمستخدم
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public DateTime EnrolledAt { get; set; }
    }

}
