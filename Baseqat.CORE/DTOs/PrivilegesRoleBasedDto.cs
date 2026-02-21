using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class PrivilegesRoleBasedCreateDto
    {
        [Required]
        public int PrivilegesId { get; set; }
        [Required]
        public string RoleId { get; set; } = string.Empty;
        public bool is_displayed { get; set; }
        public bool is_insert { get; set; }
        public bool is_update { get; set; }
        public bool is_delete { get; set; }
        public bool is_print { get; set; }
    }
    #endregion

    #region Update DTO
    public class PrivilegesRoleBasedUpdateDto
    {
        public bool? is_displayed { get; set; }
        public bool? is_insert { get; set; }
        public bool? is_update { get; set; }
        public bool? is_delete { get; set; }
        public bool? is_print { get; set; }
    }
    #endregion

    #region Response DTO
    public class PrivilegesRoleBasedDto
    {
        public int id { get; set; }
        public bool is_displayed { get; set; }
        public bool is_insert { get; set; }
        public bool is_update { get; set; }
        public bool is_delete { get; set; }
        public bool is_print { get; set; }
        public int PrivilegesId { get; set; }
        public string PrivilegeName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
    #endregion

    #region Filter DTO
    public class PrivilegesRoleBasedFilterDto
    {
        public int? id { get; set; }
        public int? PrivilegesId { get; set; }
        public string? RoleId { get; set; }
    }
    #endregion
}
