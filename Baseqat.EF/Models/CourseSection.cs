using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class CourseSection
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }

        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
    }

}
