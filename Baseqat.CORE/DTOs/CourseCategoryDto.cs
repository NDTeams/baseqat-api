using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class CourseCategoryCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
    #endregion

    #region Update DTO
    public class CourseCategoryUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
    #endregion

    #region Response DTO
    public class CourseCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
    #endregion

    #region Detail DTO
    public class CourseCategoryDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CoursesCount { get; set; }
        public List<CourseSummaryDto> Courses { get; set; } = new();
    }

    public class CourseSummaryDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
    #endregion

    #region Filter DTO
    public class CourseCategoryFilterDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
    #endregion
}
