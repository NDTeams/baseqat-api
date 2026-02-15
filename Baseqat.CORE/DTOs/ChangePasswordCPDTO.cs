using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.DTOs
{
    public class ChangePasswordCPDTO
    {
        public string userId { get; set; }
        public string NewPassword { get; set; }
    }
}
