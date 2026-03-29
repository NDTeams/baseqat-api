using System.ComponentModel.DataAnnotations;

namespace Baseqat.EF.Models
{
    public class HomeStatistic
    {
        public long Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Value { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Icon { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
