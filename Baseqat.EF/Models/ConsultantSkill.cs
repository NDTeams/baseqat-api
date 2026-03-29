namespace Baseqat.EF.Models
{
    public class ConsultantSkill
    {
        public long Id { get; set; }
        public long? ConsultantId { get; set; }
        public Consultant? Consultant { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
