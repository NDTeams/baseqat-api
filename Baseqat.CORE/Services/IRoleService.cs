using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response.Pagination;
using Baseqat.EF.Consts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Services
{
    public interface IRoleService
    {
        Task<PagedResponse<string>> 
            GetRolesForUserAsync(string userId, int PageNumber = PaginationConstants.PageNumber, 
            int PageSize = PaginationConstants.PageSize);

        Task<bool> RoleExistsAsync(string roleName);

        Task<List<_Role_PriviligeDto>> GetAllUserPriviliges(string userId);

        Task<bool> AddPriviligesToUserAsync(string userId, List<_Role_PriviligeDto> Permissions);
    }
}
