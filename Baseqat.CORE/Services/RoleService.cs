using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response.Pagination;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Services
{
    public class RoleService:IRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UsersHelper _UsersHelper;
        private readonly AppDbContext _db;
       
        public RoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            AppDbContext db , UsersHelper UsersHelper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _UsersHelper = UsersHelper;
            _db = db;
           
        }

        public async Task<PagedResponse<string>> GetRolesForUserAsync(string userId, int PageNumber = PaginationConstants.PageNumber, int PageSize = PaginationConstants.PageSize)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return PagedResponse<string>.Fail("المستخدم غير موجود");

            var roles = await _userManager.GetRolesAsync(user);
            return PagedResponse<string>.Success(roles.ToList(), PageSize, PageNumber);
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<List<_Role_PriviligeDto>> GetAllUserPriviliges(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            var prvLst = _db.Privileges.ToList();
            var olst = new List<_Role_PriviligeDto>();
            var roleId = await _UsersHelper.getUserRoleId(user.Id);
            foreach (var prv in prvLst)
            {
                Privileges_RoleBased rolePriviliges = null;
                if (!string.IsNullOrEmpty(roleId))
                {
                    rolePriviliges = _db.Privileges_RoleBased.FirstOrDefault(pr => pr.RoleId == roleId && pr.PrivilegesId == prv.Id);
                }

                var existingPriviliges = _db.Privileges_UserBased.FirstOrDefault(pr => pr.UserId == user.Id && pr.PrivilegesId == prv.Id);
                if (existingPriviliges == null)
                {
                    var newPriviliges = new _Role_PriviligeDto
                    {
                        UserId = user.Id,
                        name = prv.priv_name,
                        PrivilegesId = prv.Id,
                        is_displayed = (rolePriviliges == null ? false : rolePriviliges.is_displayed),
                        is_insert = (rolePriviliges == null ? false : rolePriviliges.is_insert),
                        is_update = (rolePriviliges == null ? false : rolePriviliges.is_update),
                        is_delete = (rolePriviliges == null ? false : rolePriviliges.is_delete),
                        is_print = (rolePriviliges == null ? false : rolePriviliges.is_print),
                        RoleId = roleId

                    };
                    olst.Add(newPriviliges);
                }
                else
                {
                    var newPriviliges = new _Role_PriviligeDto
                    {
                        UserId = user.Id,
                        name = prv.priv_name,
                        PrivilegesId = prv.Id,
                        is_displayed = existingPriviliges.is_displayed,
                        is_insert = existingPriviliges.is_insert,
                        is_update = existingPriviliges.is_update,
                        is_delete = existingPriviliges.is_delete,
                        is_print = existingPriviliges.is_print,
                        RoleId = roleId
                    };
                    olst.Add(newPriviliges);
                }
            }
            return olst;
        }

        public async Task<bool> AddPriviligesToUserAsync(string userId, List<_Role_PriviligeDto> Permissions)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            foreach (var permission in Permissions)
            {
                var existingPriviliges = _db.Privileges_UserBased.FirstOrDefault(pr => pr.UserId == user.Id && pr.PrivilegesId == permission.PrivilegesId);
                if (existingPriviliges != null)
                {
                    existingPriviliges.is_displayed = permission.is_displayed;
                    existingPriviliges.is_insert = permission.is_insert;
                    existingPriviliges.is_update = permission.is_update;
                    existingPriviliges.is_delete = permission.is_delete;
                    existingPriviliges.is_print = permission.is_print;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var newPriviliges = new Privileges_UserBased
                    {
                        UserId = user.Id,
                        PrivilegesId = permission.PrivilegesId,
                        is_displayed = permission.is_displayed,
                        is_insert = permission.is_insert,
                        is_update = permission.is_update,
                        is_delete = permission.is_delete,
                        is_print = permission.is_print,
                        
                    };
                    _db.Privileges_UserBased.Add(newPriviliges);
                    await _db.SaveChangesAsync();
                }
            }

            return true;
        }
    }
}
