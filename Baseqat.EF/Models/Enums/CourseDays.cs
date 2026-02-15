using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models.Enums
{
    [Flags]
    public enum CourseDays
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }

}
