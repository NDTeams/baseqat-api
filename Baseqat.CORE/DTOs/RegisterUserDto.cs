using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Baseqat.CORE.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"^05\d{8}$", ErrorMessage = "Phone number must be a valid Saudi number starting with 05.")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
