using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace Baseqat.EF.Models.Auth
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(150)]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public DateTime? DateOfBirth { get; set; }

        //[MaxLength(150)]
        //public string? Bio { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }
}
