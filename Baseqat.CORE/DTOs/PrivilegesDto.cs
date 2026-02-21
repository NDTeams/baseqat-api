using System.ComponentModel.DataAnnotations;

namespace Baseqat.CORE.DTOs
{
    #region Create DTO
    public class PrivilegesCreateDto
    {
        [Required]
        public string priv_name { get; set; } = string.Empty;
        [Required]
        public string priv_cat { get; set; } = string.Empty;
        public bool? isEnabled { get; set; } = true;
    }
    #endregion

    #region Update DTO
    public class PrivilegesUpdateDto
    {
        public string? priv_name { get; set; }
        public string? priv_cat { get; set; }
        public bool? isEnabled { get; set; }
    }
    #endregion

    #region Response DTO
    public class PrivilegesDto
    {
        public int Id { get; set; }
        public string priv_name { get; set; } = string.Empty;
        public Guid priv_key { get; set; }
        public string priv_cat { get; set; } = string.Empty;
        public bool? isEnabled { get; set; }
    }
    #endregion

    #region Detail DTO
    public class PrivilegesDetailDto : PrivilegesDto
    {
        public List<PrivilegesRoleBasedDto> RoleBasedPrivileges { get; set; } = new();
        public List<PrivilegesUserBasedDto> UserBasedPrivileges { get; set; } = new();
    }
    #endregion

    #region Filter DTO
    public class PrivilegesFilterDto
    {
        public int? Id { get; set; }
        public string? priv_name { get; set; }
        public string? priv_cat { get; set; }
        public bool? isEnabled { get; set; }
    }
    #endregion
}
