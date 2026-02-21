using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.CORE.Services;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models.Auth;
using Baseqt.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UsersHelper _usersHelper;
        private readonly IDataUnit _unitOfWork;
        private readonly AppDbContext _db;

        public RoleController(
            IRoleService roleService,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            UsersHelper usersHelper,
            IDataUnit unitOfWork,
            AppDbContext db)
        {
            _roleService = roleService;
            _roleManager = roleManager;
            _userManager = userManager;
            _usersHelper = usersHelper;
            _unitOfWork = unitOfWork;
            _db = db;
        }

        #region 1. Get All Roles
        [HttpGet("GetAll")]
        [isAllowed("ادارة المجموعات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var roles = _roleManager.Roles.ToList();
            
            if (roles == null || !roles.Any())
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = roles.Select(r => new
            {
                r.Id,
                r.Name,
                r.NormalizedName
            }).ToList();

            return Ok(ApiBaseResponse<object>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get Role by ID
        [HttpGet("{id}")]
        [isAllowed("ادارة المجموعات", "is_displayed")]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

            var dto = new
            {
                role.Id,
                role.Name,
                role.NormalizedName,
                UsersCount = usersInRole.Count
            };

            return Ok(ApiBaseResponse<object>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 3. Create Role
        [HttpPost("Add")]
        [isAllowed("ادارة المجموعات", "is_insert")]
        public async Task<IActionResult> Add([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest(ApiBaseResponse<string>.Fail("المجموعة موجودة مسبقاً"));

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            var dto = new
            {
                role.Id,
                role.Name,
                role.NormalizedName
            };

            return Ok(ApiBaseResponse<object>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 4. Update Role
        [HttpPut("Update/{id}")]
        [isAllowed("ادارة المجموعات", "is_update")]
        public async Task<IActionResult> Update(string id, [FromBody] string newRoleName)
        {
            if (string.IsNullOrWhiteSpace(newRoleName))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var role = await _roleManager.FindByIdAsync(id);
            
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Check if new name already exists
            var existingRole = await _roleManager.FindByNameAsync(newRoleName);
            if (existingRole != null && existingRole.Id != id)
                return BadRequest(ApiBaseResponse<string>.Fail("اسم المجموعة موجود مسبقاً"));

            role.Name = newRoleName;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            var dto = new
            {
                role.Id,
                role.Name,
                role.NormalizedName
            };

            return Ok(ApiBaseResponse<object>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 5. Delete Role
        [HttpDelete("Delete/{id}")]
        [isAllowed("ادارة المجموعات", "is_delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Check if role has users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
                return BadRequest(ApiBaseResponse<string>.Fail("لا يمكن حذف المجموعة لأنها تحتوي على مستخدمين"));

            // Delete role privileges first
            var rolePrivileges = _db.Privileges_RoleBased.Where(p => p.RoleId == id).ToList();
            if (rolePrivileges.Any())
            {
                _db.Privileges_RoleBased.RemoveRange(rolePrivileges);
                await _db.SaveChangesAsync();
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 6. Get Role Privileges
        [HttpGet("GetRolePrivileges/{roleId}")]
        [isAllowed("ادارة المجموعات", "is_displayed")]
        public async Task<IActionResult> GetRolePrivileges(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail("المجموعة غير موجودة"));

            var allPrivileges = await _unitOfWork.Privileges.GetAllAsync();
            var rolePrivileges = _db.Privileges_RoleBased.Where(p => p.RoleId == roleId).ToList();

            var dtos = allPrivileges.Select(prv =>
            {
                var rolePriv = rolePrivileges.FirstOrDefault(rp => rp.PrivilegesId == prv.Id);
                return new PrivilegesRoleBasedDto
                {
                    id = rolePriv?.id ?? 0,
                    PrivilegesId = prv.Id,
                    PrivilegeName = prv.priv_name,
                    RoleId = roleId,
                    RoleName = role.Name ?? string.Empty,
                    is_displayed = rolePriv?.is_displayed ?? false,
                    is_insert = rolePriv?.is_insert ?? false,
                    is_update = rolePriv?.is_update ?? false,
                    is_delete = rolePriv?.is_delete ?? false,
                    is_print = rolePriv?.is_print ?? false
                };
            }).ToList();

            return Ok(ApiBaseResponse<List<PrivilegesRoleBasedDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 7. Add or Update Role Privileges
        [HttpPost("AddOrUpdateRolePrivileges/{roleId}")]
        [isAllowed("ادارة المجموعات", "is_update")]
        public async Task<IActionResult> AddOrUpdateRolePrivileges(string roleId, [FromBody] List<PrivilegesRoleBasedCreateDto> privileges)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail("المجموعة غير موجودة"));

            foreach (var priv in privileges)
            {
                var existing = _db.Privileges_RoleBased
                    .FirstOrDefault(p => p.RoleId == roleId && p.PrivilegesId == priv.PrivilegesId);

                if (existing != null)
                {
                    existing.is_displayed = priv.is_displayed;
                    existing.is_insert = priv.is_insert;
                    existing.is_update = priv.is_update;
                    existing.is_delete = priv.is_delete;
                    existing.is_print = priv.is_print;
                }
                else
                {
                    var newPriv = new Privileges_RoleBased
                    {
                        RoleId = roleId,
                        PrivilegesId = priv.PrivilegesId,
                        is_displayed = priv.is_displayed,
                        is_insert = priv.is_insert,
                        is_update = priv.is_update,
                        is_delete = priv.is_delete,
                        is_print = priv.is_print
                    };
                    _db.Privileges_RoleBased.Add(newPriv);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataSaved));
        }
        #endregion

        #region 8. Get Users in Role
        [HttpGet("GetUsersInRole/{roleId}")]
        [isAllowed("ادارة المجموعات", "is_displayed")]
        public async Task<IActionResult> GetUsersInRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail("المجموعة غير موجودة"));

            var users = await _userManager.GetUsersInRoleAsync(role.Name!);

            var dtos = users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FullName,
                u.PhoneNumber
            }).ToList();

            return Ok(ApiBaseResponse<object>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 9. Assign Role to User
        [HttpPost("AssignRoleToUser")]
        [isAllowed("ادارة المجموعات", "is_update")]
        public async Task<IActionResult> AssignRoleToUser([FromQuery] string userId, [FromQuery] string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail("المستخدم غير موجود"));

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail("المجموعة غير موجودة"));

            var result = await _usersHelper.AddUserToRole(userId, role.Name!);

            if (!result)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, "تم تعيين المجموعة للمستخدم بنجاح"));
        }
        #endregion

        #region 10. Remove Role from User
        [HttpPost("RemoveRoleFromUser")]
        [isAllowed("ادارة المجموعات", "is_update")]
        public async Task<IActionResult> RemoveRoleFromUser([FromQuery] string userId, [FromQuery] string roleId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail("المستخدم غير موجود"));

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(ApiBaseResponse<string>.Fail("المجموعة غير موجودة"));

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, "تم إزالة المجموعة من المستخدم بنجاح"));
        }
        #endregion

        #region 11. Get User Roles
        [HttpGet("GetUserRoles/{userId}")]
        [isAllowed("ادارة المجموعات", "is_displayed")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail("المستخدم غير موجود"));

            var roles = await _userManager.GetRolesAsync(user);

            var roleDetails = new List<object>();
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    roleDetails.Add(new
                    {
                        role.Id,
                        role.Name
                    });
                }
            }

            return Ok(ApiBaseResponse<object>.Success(roleDetails, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 12. Check Role Exists
        [HttpGet("RoleExists/{roleName}")]
        [isAllowed("ادارة المجموعات", "is_displayed")]
        public async Task<IActionResult> RoleExists(string roleName)
        {
            var exists = await _roleService.RoleExistsAsync(roleName);
            return Ok(ApiBaseResponse<bool>.Success(exists, ResponseMessages.DataRetrieved));
        }
        #endregion
    }
}
