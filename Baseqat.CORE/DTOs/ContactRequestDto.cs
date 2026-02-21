using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    public class ContactRequestCreateDto
    {
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
    }

    public class ContactRequestStatusUpdateDto
    {
        [Required]
        public ContactRequestStatus Status { get; set; }
    }

    public class ContactRequestReplyDto
    {
        [Required]
        public ReplyChannel ReplyChannel { get; set; }

        [Required, MaxLength(2000)]
        public string ReplyMessage { get; set; } = string.Empty;

        public bool CloseRequest { get; set; }
    }

    public class ContactRequestFilterDto
    {
        public long? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RequestType { get; set; }
        public ContactRequestStatus? Status { get; set; }
        public ReplyChannel? PreferredReplyChannel { get; set; }
        public ReplyChannel? RepliedVia { get; set; }
    }

    public class ContactRequestDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ReplyChannel? PreferredReplyChannel { get; set; }
        public ContactRequestStatus Status { get; set; }
        public string? AdminReplyMessage { get; set; }
        public ReplyChannel? RepliedVia { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RepliedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? HandledBy { get; set; }
    }
}