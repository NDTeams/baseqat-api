using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class CourseInstructor
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public long InstructorId { get; set; }
        public Instructor Instructor { get; set; } = null!;
    }

}
