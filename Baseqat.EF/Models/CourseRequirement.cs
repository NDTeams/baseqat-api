using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models
{
    public class CourseRequirement
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;

        public int Order { get; set; }

        public long CourseId { get; set; }
    }

}
