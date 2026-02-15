using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.DTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Token { get; set; } // التوكن الذي أرسل في الرابط
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
