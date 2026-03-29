using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.CORE.Services;
using Baseqat.EF.Consts;
using Baseqat.EF.Models.Auth;
using Baseqt.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRoleService _roleService;
        private readonly UsersHelper _usersHelper;

        public UsersManagementController(UserManager<ApplicationUser> userManager, IRoleService roleService, UsersHelper usersHelper)
        {
            _userManager = userManager;
            _roleService = roleService;
            _usersHelper = usersHelper;
        }

        [HttpGet("AllUsers")]
        //[Authorize]
        //[isAllowed("ادارة المستخدمين", "is_displayed")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            if (users == null || !users.Any())
                return NotFound(ApiBaseResponse<string>
                    .Fail(ResponseMessages.NotFound));

            var dtos = new List<object>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                dtos.Add(new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.FullName,
                    Roles = roles.ToList(),
                    JoinedDate = u.JoinedDate.ToString("dd-MM-yyyy"),
                    isConfirmed = (u.PhoneNumberConfirmed ? "مفعل" : "غير مفعل"),
                    isLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    userImage = string.IsNullOrEmpty(u.ProfilePictureUrl) ? null : u.ProfilePictureUrl
                });
            }

            return Ok(ApiBaseResponse<IEnumerable<object>>.Success(dtos, ResponseMessages.DataRetrieved));
        }

        [HttpPost("AddBaseqatEmployee")]
        public async Task<ActionResult<object>> AddBaseqatEmployee(RegisterUserDto registerDto)
        {
            if (registerDto == null)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                FullName = registerDto.FullName,
                JoinedDate = DateTime.UtcNow,
                EmailConfirmed = true, // Assuming email is confirmed upon registration
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            // Assign role to user
            await _usersHelper.AddUserToRole(user.Id, "BaseqatEmployee");

            return Ok(ApiBaseResponse<object>.Success(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
            }, ResponseMessages.DataSaved));
        }

        [HttpPut("UpdateBaseqatEmployee/{id}")]
        public async Task<ActionResult<object>>
            UpdateBaseqatEmployee(string id, UpdateBaseqatEmployeeDto updateDto)
        {
            if (updateDto == null)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            user.FullName = updateDto.FullName;
            user.Email = updateDto.Email;
            user.UserName = updateDto.Email;
            user.PhoneNumber = updateDto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            // If password is provided, reset it
            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, updateDto.Password);

                if (!passResult.Succeeded)
                    return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                        "تم تحديث البيانات لكن فشل تغيير كلمة المرور", passResult.Errors.Select(e => e.Description).ToArray()));
            }

            return Ok(ApiBaseResponse<object>.Success(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                user.PhoneNumber
            }, ResponseMessages.DataSaved));
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<ActionResult<object>> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.DataDeleted));
        }

        [HttpGet("GetAllUserPriviliges")]
        public async Task<ActionResult<List<_Role_PriviligeDto>>> GetAllUserPriviliges([FromQuery] string userId)
        {
            return await _roleService.GetAllUserPriviliges(userId);
        }

        [HttpPost("AddOrUpdatePriviligesToUser")]
        public async Task<ActionResult<ApiBaseResponse<string>>> AddOrUpdatePriviligesToUser([FromQuery] string userId, List<_Role_PriviligeDto> permissions)
        {
            var r = await _roleService.AddPriviligesToUserAsync(userId, permissions);
            if (!r)
            {
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));
            }
            else
            {
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.DataSaved));
            }
        }

        [HttpGet("GetUserInfo")]
        public async Task<ActionResult<ApiBaseResponse<object>>> GetUserInfo([FromQuery] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var userInfo = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.UserName,
                JoinedDate = user.JoinedDate.ToString("dd-MM-yyyy") ?? "",
                isConfirmed = user.PhoneNumberConfirmed,

            };

            return Ok(ApiBaseResponse<object>.Success(userInfo, ResponseMessages.DataRetrieved));
        }

        [HttpPost("UpdateUserInfo")]
        public async Task<ActionResult<ApiBaseResponse<object>>> UpdateUserInfo([FromQuery] string userId,  UpdateUserSimpleDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.FullName))
                user.FullName = model.FullName;

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                // Optional: Check if phone number is taken by another user
                var existingUser = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.PhoneNumber && u.Id != userId);
                if (existingUser != null)
                    return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.PhoneNumberInUse));

                user.PhoneNumber = model.PhoneNumber;
            }

         

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<object>.Success(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber
            }, ResponseMessages.DataSaved));
        }

        [HttpPost("UpdateUserProfilePhoto")]
        public async Task<ActionResult<ApiBaseResponse<object>>> UpdateUserProfilePhoto([FromQuery] string userId, IFormFile image)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (image == null || image.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            // Validate image type
            if (!image.ContentType.ToLower().StartsWith("image/"))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidImageFileType));

            // Validate size (max 2MB for example, attempting to match existing standard or set reasonable limit)
            const long maxFileSize = 2 * 1024 * 1024; // 2 MB
            if (image.Length > maxFileSize)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.FileSizeExceeded));

            // Save the image
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "users");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // Delete old image if exists and not default (if applicable)
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldImagePath))
                {
                    try { System.IO.File.Delete(oldImagePath); } catch { /* ignore */ }
                }
            }

            user.ProfilePictureUrl = $"/img/users/{uniqueFileName}";
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<object>.Success(new
            {
                user.Id,
                user.ProfilePictureUrl
            }, ResponseMessages.DataSaved));
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult<ApiBaseResponse<object>>> ResetPassword(ChangePasswordCPDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<string>.Success(null, "تم تغيير كلمة المرور بنجاح"));
        }

        [HttpPost("LockUser")]
        public async Task<ActionResult<ApiBaseResponse<object>>> LockUser([FromQuery] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<string>.Success(null, "تم قفل حساب المستخدم بنجاح"));
        }

        [HttpPost("UnlockUser")]
        public async Task<ActionResult<ApiBaseResponse<object>>> UnlockUser([FromQuery] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<string>.Success(null, "تم فك قفل حساب المستخدم بنجاح"));
        }

     
        [HttpPost("SoftDeleteAccount")]
        public async Task<ActionResult<ApiBaseResponse<object>>> SoftDeleteAccount([FromQuery] string? userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = _userManager.GetUserId(User);
            }

            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            if (!result.Succeeded)
                return BadRequest(ApiBaseResponse<IEnumerable<string>>.Fail(
                    ResponseMessages.OperationFailed, result.Errors.Select(e => e.Description).ToArray()));

            return Ok(ApiBaseResponse<string>.Success(null, "تم قفل حساب المستخدم بنجاح"));
        }

        [HttpGet("ListUsersByRoleName")]
        public async Task<ActionResult<ApiBaseResponse<List<object>>>> ListUsersByRoleName([FromQuery] string roleName)
        {
            var users = await _usersHelper.getUsersByRoleName(roleName);
            if (users == null || !users.Any())
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                u.FullName,
              
                JoinedDate = u.JoinedDate.ToString("dd-MM-yyyy") ?? "",
                isConfirmed = (u.PhoneNumberConfirmed ? "مفعل" : "غير مفعل"),
                isLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                userImage = string.IsNullOrEmpty(u.ProfilePictureUrl) ? null : u.ProfilePictureUrl//$"{Request.Scheme}://{Request.Host}{u.UserImage}"
            }).ToList<object>();

            return Ok(ApiBaseResponse<List<object>>.Success(dtos, ResponseMessages.DataRetrieved));
        }

    }
}
