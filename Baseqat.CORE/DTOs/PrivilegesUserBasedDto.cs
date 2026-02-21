using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class PrivilegesUserBasedCreateDto
    {
        [Required]
        public int PrivilegesId { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public long? companyId { get; set; }
        public bool is_displayed { get; set; }
        public bool is_insert { get; set; }
        public bool is_update { get; set; }
        public bool is_delete { get; set; }
        public bool is_print { get; set; }
    }
    #endregion

    #region Update DTO
    public class PrivilegesUserBasedUpdateDto
    {
        public bool? is_displayed { get; set; }
        public bool? is_insert { get; set; }
        public bool? is_update { get; set; }
        public bool? is_delete { get; set; }
        public bool? is_print { get; set; }
        public long? companyId { get; set; }
    }
    #endregion

    #region Response DTO
    public class PrivilegesUserBasedDto
    {
        public int id { get; set; }
        public bool is_displayed { get; set; }
        public bool is_insert { get; set; }
        public bool is_update { get; set; }
        public bool is_delete { get; set; }
        public bool is_print { get; set; }
        public int PrivilegesId { get; set; }
        public string PrivilegeName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public long? companyId { get; set; }
    }
    #endregion

    #region Filter DTO
    public class PrivilegesUserBasedFilterDto
    {
        public int? id { get; set; }
        public int? PrivilegesId { get; set; }
        public string? UserId { get; set; }
        public long? companyId { get; set; }
    }
    #endregion
}
