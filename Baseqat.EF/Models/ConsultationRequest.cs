using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using System;

namespace Baseqat.EF.Models
{
    public class ConsultationRequest : AuditableEntity
    {
        public long Id { get; set; }

        // المستشار (يُعيّن لاحقاً من قِبَل الإدارة)
        public long? ConsultantId { get; set; }
        public Consultant? Consultant { get; set; }

        // نوع الاستشارة
        public long ConsultationCategoryId { get; set; }
        public ConsultationCategory? ConsultationCategory { get; set; }

        // العميل (مسجل الدخول)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // بيانات العميل (تُنسخ تلقائياً من الحساب)
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;

        // تفاصيل الطلب
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTime { get; set; }

        // حالة الطلب
        public ConsultationRequestStatus Status { get; set; } = ConsultationRequestStatus.PendingAssignment;
        public string? AdminNotes { get; set; }

        // رد المستشار
        public ConsultantResponseType ConsultantResponse { get; set; } = ConsultantResponseType.None;
        public string? ConsultantNotes { get; set; }
        public DateTime? SuggestedDate { get; set; }
        public string? SuggestedTime { get; set; }

        // رابط الزوم (يُضاف من قِبَل موظفي باسقات)
        public string? ZoomLink { get; set; }
    }
}
