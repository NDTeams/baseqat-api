using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;
using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientProfileController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ClientProfileController(
            IDataUnit unitOfWork,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _env = env;
            _configuration = configuration;
        }

        #region 1. Get My Profile
        [HttpGet("GetMyProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // جلب أو إنشاء ClientProfile تلقائياً
            var profile = await _unitOfWork.ClientProfile.FindAsync(x => x.UserId == userId);
            if (profile == null)
            {
                profile = new ClientProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _unitOfWork.ClientProfile.AddAsync(profile);
                await _unitOfWork.CompleteAsync();
            }

            var dto = MapToDto(user, profile);
            return Ok(ApiBaseResponse<ClientProfileDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Update My Profile
        [HttpPut("UpdateMyProfile")]
        public async Task<IActionResult> UpdateMyProfile(UpdateClientProfileDto model)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var profile = await _unitOfWork.ClientProfile.FindAsync(x => x.UserId == userId);
            if (profile == null)
            {
                profile = new ClientProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _unitOfWork.ClientProfile.AddAsync(profile);
                await _unitOfWork.CompleteAsync();
            }

            // تحديث بيانات ApplicationUser
            if (model.FullName != null)
                user.FullName = model.FullName;

            if (model.PhoneNumber != null)
            {
                var existingUser = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.PhoneNumber && u.Id != userId);
                if (existingUser != null)
                    return BadRequest(ApiBaseResponse<string>.Fail("رقم الهاتف مستخدم بالفعل"));
                user.PhoneNumber = model.PhoneNumber;
            }

            if (model.DateOfBirth.HasValue)
                user.DateOfBirth = model.DateOfBirth;

            var userResult = await _userManager.UpdateAsync(user);
            if (!userResult.Succeeded)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // تحديث بيانات ClientProfile
            if (model.Bio != null) profile.Bio = model.Bio;
            if (model.Address != null) profile.Address = model.Address;
            if (model.Gender.HasValue) profile.Gender = model.Gender.Value;
            if (model.Skills != null) profile.SkillsJson = JsonSerializer.Serialize(model.Skills);
            if (model.Interests != null) profile.InterestsJson = JsonSerializer.Serialize(model.Interests);
            if (model.LinkedInUrl != null) profile.LinkedInUrl = model.LinkedInUrl;
            if (model.XUrl != null) profile.XUrl = model.XUrl;
            if (model.InstagramUrl != null) profile.InstagramUrl = model.InstagramUrl;
            if (model.FacebookUrl != null) profile.FacebookUrl = model.FacebookUrl;
            if (model.GitHubUrl != null) profile.GitHubUrl = model.GitHubUrl;
            if (model.WebsiteUrl != null) profile.WebsiteUrl = model.WebsiteUrl;

            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            _unitOfWork.ClientProfile.Update(profile);
            await _unitOfWork.CompleteAsync();

            var dto = MapToDto(user, profile);
            return Ok(ApiBaseResponse<ClientProfileDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 3. Upload Avatar
        [HttpPost("UploadAvatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // التحقق من نوع الملف
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiBaseResponse<string>.Fail("نوع الملف غير مسموح به. الأنواع المسموحة: JPG, PNG, WEBP"));

            // التحقق من الحجم (2MB)
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(ApiBaseResponse<string>.Fail("حجم الملف يجب أن لا يتجاوز 2 ميجابايت"));

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "clients", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // حذف الصورة القديمة
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldPath = GetPhysicalPath(user.ProfilePictureUrl);
                if (!string.IsNullOrEmpty(oldPath) && System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = BuildPublicUrl($"/uploads/clients/avatars/{fileName}");
            await _userManager.UpdateAsync(user);

            return Ok(ApiBaseResponse<object>.Success(
                new { ProfilePictureUrl = user.ProfilePictureUrl },
                ResponseMessages.DataSaved));
        }
        #endregion

        #region 3.1 Delete Avatar
        [HttpDelete("DeleteAvatar")]
        public async Task<IActionResult> DeleteAvatar()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (string.IsNullOrEmpty(user.ProfilePictureUrl))
                return Ok(ApiBaseResponse<string>.Success(null, "لا توجد صورة لحذفها"));

            var oldPath = GetPhysicalPath(user.ProfilePictureUrl);
            if (!string.IsNullOrEmpty(oldPath) && System.IO.File.Exists(oldPath))
            {
                try { System.IO.File.Delete(oldPath); } catch { }
            }

            user.ProfilePictureUrl = null;
            await _userManager.UpdateAsync(user);

            return Ok(ApiBaseResponse<string>.Success(null, "تم حذف الصورة الشخصية بنجاح"));
        }
        #endregion

        #region 4. Upload CV
        [HttpPost("UploadCv")]
        public async Task<IActionResult> UploadCv(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var profile = await _unitOfWork.ClientProfile.FindAsync(x => x.UserId == userId);
            if (profile == null)
            {
                profile = new ClientProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _unitOfWork.ClientProfile.AddAsync(profile);
                await _unitOfWork.CompleteAsync();
            }

            // التحقق من نوع الملف
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiBaseResponse<string>.Fail("نوع الملف غير مسموح به. الأنواع المسموحة: PDF, DOC, DOCX"));

            // التحقق من الحجم (5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(ApiBaseResponse<string>.Fail("حجم الملف يجب أن لا يتجاوز 5 ميجابايت"));

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "clients", "cv");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // حذف السيرة الذاتية القديمة
            if (!string.IsNullOrEmpty(profile.CvUrl))
            {
                var oldPath = GetPhysicalPath(profile.CvUrl);
                if (!string.IsNullOrEmpty(oldPath) && System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            profile.CvUrl = BuildPublicUrl($"/uploads/clients/cv/{fileName}");
            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            _unitOfWork.ClientProfile.Update(profile);
            await _unitOfWork.CompleteAsync();

            return Ok(ApiBaseResponse<object>.Success(
                new { CvUrl = profile.CvUrl },
                ResponseMessages.DataSaved));
        }
        #endregion

        #region 5. Delete CV
        [HttpDelete("DeleteCv")]
        public async Task<IActionResult> DeleteCv()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            var profile = await _unitOfWork.ClientProfile.FindAsync(x => x.UserId == userId);
            if (profile == null || string.IsNullOrEmpty(profile.CvUrl))
                return Ok(ApiBaseResponse<string>.Fail("لا توجد سيرة ذاتية لحذفها"));

            var oldPath = GetPhysicalPath(profile.CvUrl);
            if (!string.IsNullOrEmpty(oldPath) && System.IO.File.Exists(oldPath))
            {
                try { System.IO.File.Delete(oldPath); } catch { }
            }

            profile.CvUrl = null;
            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            _unitOfWork.ClientProfile.Update(profile);
            await _unitOfWork.CompleteAsync();

            return Ok(ApiBaseResponse<string>.Success(null, "تم حذف السيرة الذاتية بنجاح"));
        }
        #endregion

        #region 6. Change Password
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (model.NewPassword != model.ConfirmNewPassword)
                return BadRequest(ApiBaseResponse<string>.Fail("كلمة المرور الجديدة غير متطابقة مع التأكيد"));

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(ApiBaseResponse<string>.Fail("فشل تغيير كلمة المرور", errors));
            }

            return Ok(ApiBaseResponse<string>.Success(null, "تم تغيير كلمة المرور بنجاح"));
        }
        #endregion

        #region Helpers
        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
        }

        private ClientProfileDto MapToDto(ApplicationUser user, ClientProfile profile)
        {
            var skills = new List<string>();
            var interests = new List<string>();

            if (!string.IsNullOrEmpty(profile.SkillsJson))
            {
                try { skills = JsonSerializer.Deserialize<List<string>>(profile.SkillsJson) ?? new(); } catch { }
            }
            if (!string.IsNullOrEmpty(profile.InterestsJson))
            {
                try { interests = JsonSerializer.Deserialize<List<string>>(profile.InterestsJson) ?? new(); } catch { }
            }

            return new ClientProfileDto
            {
                Id = profile.Id,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                ProfilePictureUrl = BuildPublicUrl(user.ProfilePictureUrl),
                JoinedDate = user.JoinedDate,
                Bio = profile.Bio,
                Address = profile.Address,
                Gender = profile.Gender,
                GenderName = profile.Gender.ToString(),
                CvUrl = BuildPublicUrl(profile.CvUrl),
                Skills = skills,
                Interests = interests,
                LinkedInUrl = profile.LinkedInUrl,
                XUrl = profile.XUrl,
                InstagramUrl = profile.InstagramUrl,
                FacebookUrl = profile.FacebookUrl,
                GitHubUrl = profile.GitHubUrl,
                WebsiteUrl = profile.WebsiteUrl,
                ProfileCompletion = CalculateCompletion(user, profile),
                CompletionDetails = GetCompletionDetails(user, profile)
            };
        }

        private ProfileCompletionDetails GetCompletionDetails(ApplicationUser user, ClientProfile profile)
        {
            return new ProfileCompletionDetails
            {
                HasFullName = !string.IsNullOrWhiteSpace(user.FullName),
                HasProfilePicture = !string.IsNullOrWhiteSpace(user.ProfilePictureUrl),
                HasPhoneNumber = !string.IsNullOrWhiteSpace(user.PhoneNumber),
                HasDateOfBirth = user.DateOfBirth.HasValue,
                HasBio = !string.IsNullOrWhiteSpace(profile.Bio),
                HasAddress = !string.IsNullOrWhiteSpace(profile.Address),
                HasGender = profile.Gender != Gender.Unknown,
                HasCv = !string.IsNullOrWhiteSpace(profile.CvUrl),
            };
        }

        private int CalculateCompletion(ApplicationUser user, ClientProfile profile)
        {
            int score = 0;
            if (!string.IsNullOrWhiteSpace(user.FullName)) score += 15;
            if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl)) score += 15;
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber)) score += 15;
            if (user.DateOfBirth.HasValue) score += 10;
            if (!string.IsNullOrWhiteSpace(profile.Bio)) score += 15;
            if (!string.IsNullOrWhiteSpace(profile.Address)) score += 10;
            if (profile.Gender != Gender.Unknown) score += 10;
            if (!string.IsNullOrWhiteSpace(profile.CvUrl)) score += 10;
            return score;
        }

        private string? BuildPublicUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            var baseUrl = _configuration["AppSettings:FileBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                return url;

            var relativePath = GetRelativePath(url);
            if (string.IsNullOrWhiteSpace(relativePath))
                return url;

            return $"{baseUrl.TrimEnd('/')}{relativePath}";
        }

        private string? GetRelativePath(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
                return absoluteUri.AbsolutePath;

            if (!url.StartsWith('/'))
                return "/" + url;

            return url;
        }

        private string? GetPhysicalPath(string? url)
        {
            var relativePath = GetRelativePath(url);
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            var sanitizedPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_env.WebRootPath, sanitizedPath);
        }
        #endregion
    }
}
