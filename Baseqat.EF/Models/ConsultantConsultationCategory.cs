namespace Baseqat.EF.Models
{
    public class ConsultantConsultationCategory
    {
        public long Id { get; set; }
        public long ConsultantId { get; set; }
        public Consultant? Consultant { get; set; }
        public long ConsultationCategoryId { get; set; }
        public ConsultationCategory? ConsultationCategory { get; set; }
    }
}
