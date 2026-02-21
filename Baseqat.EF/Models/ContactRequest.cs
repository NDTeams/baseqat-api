using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.EF.Models
{
    public class ContactRequest : DeleteEntity
    {
        public long Id { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string RequestType { get; set; } = "استفسار عام";

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public ReplyChannel? PreferredReplyChannel { get; set; }

        public ContactRequestStatus Status { get; set; } = ContactRequestStatus.New;

        [MaxLength(2000)]
        public string? AdminReplyMessage { get; set; }

        public ReplyChannel? RepliedVia { get; set; }

        public DateTime? RepliedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string? HandledBy { get; set; }
    }
}