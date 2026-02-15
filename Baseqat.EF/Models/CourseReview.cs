using Baseqat.EF.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class CourseReview
    {
        public long Id { get; set; }

        // العلاقة بالكورس
        public long CourseId { get; set; }
        public Course Course { get; set; }

        // العلاقة بالمستخدم
        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public int Rating { get; set; } // 1..5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

    }

}
