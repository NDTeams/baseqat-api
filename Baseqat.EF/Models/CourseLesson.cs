using Baseqat.EF.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class CourseLesson
    {
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public LessonType LessonType { get; set; }

        public int DurationInMinutes { get; set; }

        public bool IsPreview { get; set; }

        public int Order { get; set; }

        public long CourseSectionId { get; set; }
    }
}
