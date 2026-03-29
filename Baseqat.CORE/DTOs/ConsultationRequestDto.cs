using Baseqat.EF.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO (Authenticated Client)
    public class ConsultationRequestCreateDto
    {
        [Required]
        public long ConsultationCategoryId { get; set; }
        [Required]
        public string Subject { get; set; } = string.Empty;
        [Required]
        public string Message { get; set; } = string.Empty;
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTime { get; set; }
    }
    #endregion

    #region Assign Consultant DTO (Admin)
    public class ConsultationRequestAssignDto
    {
        [Required]
        public long ConsultantId { get; set; }
        public string? AdminNotes { get; set; }
    }
    #endregion

    #region Response DTO
    public class ConsultationRequestDto
    {
        public long Id { get; set; }
        public long? ConsultantId { get; set; }
        public string? ConsultantName { get; set; }
        public long ConsultationCategoryId { get; set; }
        public string? ConsultationCategoryName { get; set; }
        public string? UserId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTime { get; set; }
        public ConsultationRequestStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public ConsultantResponseType ConsultantResponse { get; set; }
        public string? ConsultantResponseName { get; set; }
        public string? ConsultantNotes { get; set; }
        public DateTime? SuggestedDate { get; set; }
        public string? SuggestedTime { get; set; }
        public string? ZoomLink { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    #endregion

    #region Status Update DTO
    public class ConsultationRequestStatusUpdateDto
    {
        [Required]
        public ConsultationRequestStatus Status { get; set; }
        public string? AdminNotes { get; set; }
        public string? ZoomLink { get; set; }
    }
    #endregion

    #region Consultant Response DTO
    public class ConsultationRequestConsultantResponseDto
    {
        [Required]
        public ConsultantResponseType ResponseType { get; set; }
        public string? ConsultantNotes { get; set; }
        public DateTime? SuggestedDate { get; set; }
        public string? SuggestedTime { get; set; }
    }
    #endregion

    #region Filter DTO
    public class ConsultationRequestFilterDto
    {
        public long? Id { get; set; }
        public long? ConsultantId { get; set; }
        public long? ConsultationCategoryId { get; set; }
        public string? ClientName { get; set; }
        public ConsultationRequestStatus? Status { get; set; }
    }
    #endregion

    #region Calendar DTOs
    public class ConsultationRequestCalendarQueryDto
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public long? ConsultantId { get; set; }
        public ConsultationRequestStatus? Status { get; set; }
    }

    public class ConsultationRequestCalendarItemDto
    {
        public long Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string? ConsultantName { get; set; }
        public long? ConsultantId { get; set; }
        public string? ConsultationCategoryName { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTime { get; set; }
        public DateTime? SuggestedDate { get; set; }
        public string? SuggestedTime { get; set; }
        public ConsultationRequestStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? ZoomLink { get; set; }
    }

    public class ConsultantScheduleConflictDto
    {
        public bool HasConflict { get; set; }
        public List<ConsultationRequestCalendarItemDto> ConflictingRequests { get; set; } = new();
    }
    #endregion
}
