namespace Baseqat.CORE.DTOs
{
    public class HomeStatisticDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class HomeStatisticCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
    }

    public class HomeStatisticUpdateDto
    {
        public string? Title { get; set; }
        public string? Value { get; set; }
        public string? Icon { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
    }
}
