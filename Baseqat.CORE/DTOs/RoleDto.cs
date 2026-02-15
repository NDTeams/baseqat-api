using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.DTOs
{
    public class RoleDto
    {

        public string Email { get; set; }
        public string roleName { get; set; }

    }
    public class _RoleDto
    {

        public string Id { get; set; }
        public string roleName { get; set; }
        public List<string> Permissions { get; set; }
    }
    public class _Role_PriviligeDto
    {

        //public string Id { get; set; }
        public bool is_displayed { get; set; } = false;
        public bool is_insert { get; set; } = false;
        public bool is_update { get; set; } = false;
        public bool is_delete { get; set; } = false;
        public bool is_print { get; set; } = false;
        public int PrivilegesId { get; set; }
        public string RoleId { get; set; }
        public string? UserId { get; set; }
        public string name { get; set; }
    }
}
