using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    public class UpdateBaseqatEmployeeDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Optional - if provided, the password will be reset
        /// </summary>
        public string? Password { get; set; }
    }
}
