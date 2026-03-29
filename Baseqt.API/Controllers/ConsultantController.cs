using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.CORE.Response.Pagination;
using Baseqat.CORE.Services;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;
using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using Baseqt.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultantController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly UsersHelper _usersHelper;

        public ConsultantController(
            IDataUnit unitOfWork,
            IWebHostEnvironment env,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            UsersHelper usersHelper)
        {
            _unitOfWork = unitOfWork;
            _env = env;
            _configuration = configuration;
            _userManager = userManager;
            _emailService = emailService;
            _usersHelper = usersHelper;
        }

        #region 0. Get Active (Public)
        [HttpGet("GetActive")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await _unitOfWork.Consultant.FindAllAsync(
                criteria: x => x.IsDeleted != true && x.IsActive == true,
                includes: ["Skills", "ConsultantCategories.ConsultationCategory"]
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDetailDto).ToList();

            return Ok(ApiBaseResponse<List<ConsultantDetailDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }

        [HttpGet("GetActiveById/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveById(long id)
        {
            var entity = await _unitOfWork.Consultant.FindAsync(
                x => x.Id == id && x.IsDeleted != true && x.IsActive == true,
                ["Skills", "ConsultantCategories.ConsultationCategory"]
            );

            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dto = MapToDetailDto(entity);

            return Ok(ApiBaseResponse<ConsultantDetailDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.Consultant.FindAllAsync(
                criteria: x => x.IsDeleted != true,
                includes: ["Skills", "User"]
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<ConsultantDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] ConsultantFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<Consultant, bool>> criteria = x =>
                x.IsDeleted != true &&
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Name) || x.Name.Contains(filter.Name)) &&
                (string.IsNullOrEmpty(filter.Title) || x.Title.Contains(filter.Title)) &&
                (string.IsNullOrEmpty(filter.Specialty) || x.Specialty != null && x.Specialty.Contains(filter.Specialty)) &&
                (filter.Gender == null || x.Gender == filter.Gender) &&
                (filter.IsActive == null || x.IsActive == filter.IsActive);

            var totalCount = await _unitOfWork.Consultant.CountAsync(criteria);

            var result = await _unitOfWork.Consultant.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending,
                includes: ["Skills", "User"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<ConsultantDto>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<ConsultantDto>
            {
                Data = dtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                Succeeded = true,
                Message = ResponseMessages.DataRetrieved
            });
        }
        #endregion

        #region 3. Get by ID
        [HttpGet("{id}")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.Consultant.FindAsync(x => x.Id == id, ["Skills", "User"]);

            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة المستشارين", "is_insert")]
        public async Task<IActionResult> Add(ConsultantCreateDto model)
        {
            var entity = new Consultant
            {
                Name = model.Name,
                Title = model.Title,
                Bio = model.Bio ?? string.Empty,
                Gender = model.Gender,
                UserId = model.UserId,
                YearsOfExperience = model.YearsOfExperience,
                Specialty = model.Specialty,
                HourlyRate = model.HourlyRate,
                Availability = model.Availability,
                LinkedInUrl = model.LinkedInUrl,
                XUrl = model.XUrl,
                InstagramUrl = model.InstagramUrl,
                FacebookUrl = model.FacebookUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _unitOfWork.Consultant.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Load user if linked
            if (!string.IsNullOrEmpty(entity.UserId))
            {
                entity = await _unitOfWork.Consultant.FindAsync(x => x.Id == entity.Id, ["User"]);
            }

            var dto = MapToDto(entity!);
            return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 4.1 Add With Account
        [HttpPost("AddWithAccount")]
        [isAllowed("إدارة المستشارين", "is_insert")]
        public async Task<IActionResult> AddWithAccount(ConsultantCreateWithAccountDto model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                // تحقق إذا المستخدم عنده سجل مستشار بالفعل
                var existingConsultant = await _unitOfWork.Consultant.FindAsync(
                    x => x.UserId == existingUser.Id && x.IsDeleted != true);

                if (existingConsultant != null)
                    return Ok(ApiBaseResponse<string>.Fail("هذا المستخدم لديه سجل مستشار بالفعل"));

                // مستخدم موجود (مدرب مثلاً) - إضافة صلاحية مستشار مباشرة بدون حذف الأدوار الأخرى
                await _userManager.AddToRoleAsync(existingUser, IntitalRoles.Consultant);

                // إنشاء سجل المستشار مرتبط بالحساب الموجود
                var consultant = new Consultant
                {
                    Name = model.Name,
                    Title = model.Title,
                    Bio = model.Bio ?? string.Empty,
                    Gender = model.Gender,
                    UserId = existingUser.Id,
                    YearsOfExperience = model.YearsOfExperience,
                    Specialty = model.Specialty,
                    HourlyRate = model.HourlyRate,
                    Availability = model.Availability,
                    LinkedInUrl = model.LinkedInUrl,
                    XUrl = model.XUrl,
                    InstagramUrl = model.InstagramUrl,
                    FacebookUrl = model.FacebookUrl,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                await _unitOfWork.Consultant.AddAsync(consultant);
                var result = await _unitOfWork.CompleteAsync();

                if (result == 0)
                    return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

                // حفظ أقسام الاستشارات
                if (model.CategoryIds != null && model.CategoryIds.Any())
                {
                    var categoryEntities = model.CategoryIds.Distinct()
                        .Select(catId => new ConsultantConsultationCategory
                        {
                            ConsultantId = consultant.Id,
                            ConsultationCategoryId = catId
                        }).ToList();

                    await _unitOfWork.ConsultantConsultationCategory.AddRangeAsync(categoryEntities);
                    await _unitOfWork.CompleteAsync();
                }

                consultant = await _unitOfWork.Consultant.FindAsync(x => x.Id == consultant.Id, ["User"]);
                var dto = MapToDto(consultant!);
                return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, "تم إضافة المستشار وربطه بالحساب الموجود بنجاح"));
            }
            else
            {
                // مستخدم جديد - إنشاء حساب
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FullName = model.Name,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Ok(ApiBaseResponse<string>.Fail($"فشل إنشاء الحساب: {errors}"));
                }

                // إضافة صلاحية المستشار
                await _usersHelper.AddUserToRole(user.Id, IntitalRoles.Consultant);

                // إنشاء سجل المستشار
                var consultant = new Consultant
                {
                    Name = model.Name,
                    Title = model.Title,
                    Bio = model.Bio ?? string.Empty,
                    Gender = model.Gender,
                    UserId = user.Id,
                    YearsOfExperience = model.YearsOfExperience,
                    Specialty = model.Specialty,
                    HourlyRate = model.HourlyRate,
                    Availability = model.Availability,
                    LinkedInUrl = model.LinkedInUrl,
                    XUrl = model.XUrl,
                    InstagramUrl = model.InstagramUrl,
                    FacebookUrl = model.FacebookUrl,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                await _unitOfWork.Consultant.AddAsync(consultant);
                var result = await _unitOfWork.CompleteAsync();

                if (result == 0)
                {
                    // حذف المستخدم في حالة فشل إنشاء المستشار
                    await _userManager.DeleteAsync(user);
                    return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));
                }

                // حفظ أقسام الاستشارات
                if (model.CategoryIds != null && model.CategoryIds.Any())
                {
                    var categoryEntities = model.CategoryIds.Distinct()
                        .Select(catId => new ConsultantConsultationCategory
                        {
                            ConsultantId = consultant.Id,
                            ConsultationCategoryId = catId
                        }).ToList();

                    await _unitOfWork.ConsultantConsultationCategory.AddRangeAsync(categoryEntities);
                    await _unitOfWork.CompleteAsync();
                }

                consultant = await _unitOfWork.Consultant.FindAsync(x => x.Id == consultant.Id, ["User"]);

                // إرسال بريد ترحيبي
                try
                {
                    var emailBody = EmailTemplates.GetWelcomeEmail(model.Name);
                    await _emailService.SendEmailAsync(
                        new List<string> { model.Email },
                        "مرحباً بك في بصقت",
                        emailBody
                    );
                }
                catch { /* لا نفشل العملية إذا فشل إرسال البريد */ }

                var dto = MapToDto(consultant!);
                return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, ResponseMessages.DataSaved));
            }
        }
        #endregion

        #region 4.2 Check Email
        [HttpGet("CheckEmail")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Ok(ApiBaseResponse<object>.Fail("البريد الإلكتروني مطلوب"));

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user == null)
                return Ok(ApiBaseResponse<object>.Success(new { exists = false }, ResponseMessages.DataRetrieved));

            // تحقق من الأدوار
            var roles = await _userManager.GetRolesAsync(user);
            bool isInstructor = roles.Contains("Trainer") || roles.Contains("Instructor");
            bool isConsultant = roles.Contains(IntitalRoles.Consultant);

            // تحقق من وجود سجل مستشار
            var consultantRecord = await _unitOfWork.Consultant.FindAsync(
                x => x.UserId == user.Id && x.IsDeleted != true);
            bool hasConsultantRecord = consultantRecord != null;

            // جلب بيانات المدرب إن وُجد
            Instructor? instructor = null;
            if (isInstructor)
            {
                instructor = await _unitOfWork.Instructor.FindAsync(
                    x => x.UserId == user.Id && x.IsDeleted != true);
            }

            return Ok(ApiBaseResponse<object>.Success(new
            {
                exists = true,
                isInstructor,
                isConsultant,
                hasConsultantRecord,
                userName = user.FullName ?? user.UserName,
                instructorName = instructor?.Name,
                instructorTitle = instructor?.Title,
                instructorBio = instructor?.Bio,
                instructorGender = instructor?.Gender,
                instructorYearsOfExperience = instructor?.YearsOfExperience
            }, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 5. Upload Avatar Image
        [HttpPost("UploadAvatar/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> UploadAvatar(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = await _unitOfWork.Consultant.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiBaseResponse<string>.Fail("نوع الملف غير مسموح به"));

            // Create directory if not exists
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "consultants");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Delete old image if exists
            if (!string.IsNullOrEmpty(entity.AvatarUrl))
            {
                var oldImagePath = GetPhysicalPath(entity.AvatarUrl);
                if (!string.IsNullOrEmpty(oldImagePath) && System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            // Save new image
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            entity.AvatarUrl = BuildPublicUrl($"/uploads/consultants/{fileName}");
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Consultant.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<object>.Success(new { AvatarUrl = BuildPublicUrl(entity.AvatarUrl) }, ResponseMessages.DataSaved));
        }
        #endregion

        #region 5.1 Upload CV
        [HttpPost("UploadCv/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> UploadCv(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = await _unitOfWork.Consultant.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Validate file type (PDF only)
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiBaseResponse<string>.Fail("نوع الملف غير مسموح به. الأنواع المسموحة: PDF, DOC, DOCX"));

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(ApiBaseResponse<string>.Fail("حجم الملف يجب أن لا يتجاوز 5 ميجابايت"));

            // Create directory if not exists
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "consultants", "cv");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Delete old CV if exists
            if (!string.IsNullOrEmpty(entity.CvUrl))
            {
                var oldCvPath = GetPhysicalPath(entity.CvUrl);
                if (!string.IsNullOrEmpty(oldCvPath) && System.IO.File.Exists(oldCvPath))
                    System.IO.File.Delete(oldCvPath);
            }

            // Save new CV
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            entity.CvUrl = BuildPublicUrl($"/uploads/consultants/cv/{fileName}");
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Consultant.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<object>.Success(new { CvUrl = BuildPublicUrl(entity.CvUrl) }, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> Update(long id, ConsultantUpdateDto model)
        {
            var entity = await _unitOfWork.Consultant.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Update fields if provided
            if (!string.IsNullOrEmpty(model.Name)) entity.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Title)) entity.Title = model.Title;
            if (model.Bio != null) entity.Bio = model.Bio;
            if (model.Gender.HasValue) entity.Gender = model.Gender.Value;
            if (model.Rating.HasValue) entity.Rating = model.Rating.Value;
            if (model.UserId != null) entity.UserId = string.IsNullOrEmpty(model.UserId) ? null : model.UserId;
            if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;
            if (model.YearsOfExperience.HasValue) entity.YearsOfExperience = model.YearsOfExperience;
            if (model.Specialty != null) entity.Specialty = model.Specialty;
            if (model.HourlyRate.HasValue) entity.HourlyRate = model.HourlyRate.Value;
            if (model.Availability != null) entity.Availability = model.Availability;
            if (model.LinkedInUrl != null) entity.LinkedInUrl = model.LinkedInUrl;
            if (model.XUrl != null) entity.XUrl = model.XUrl;
            if (model.InstagramUrl != null) entity.InstagramUrl = model.InstagramUrl;
            if (model.FacebookUrl != null) entity.FacebookUrl = model.FacebookUrl;

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Consultant.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 7. Delete (Permanent)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة المستشارين", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.Consultant.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Delete avatar if exists
            if (!string.IsNullOrEmpty(entity.AvatarUrl))
            {
                var imagePath = GetPhysicalPath(entity.AvatarUrl);
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _unitOfWork.Consultant.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 8. Soft Delete
        [HttpPut("SoftDelete/{id}")]
        [isAllowed("إدارة المستشارين", "is_delete")]
        public async Task<IActionResult> SoftDelete(long id)
        {
            var entity = await _unitOfWork.Consultant.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Consultant.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 9. Get Deleted
        [HttpGet("GetDeleted")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _unitOfWork.Consultant.FindAllAsync(x => x.IsDeleted == true);

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<ConsultantDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 10. Restore Deleted
        [HttpPut("Restore/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> Restore(long id)
        {
            var entity = await _unitOfWork.Consultant.FindAsync(x => x.Id == id && x.IsDeleted == true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail("المستشار غير موجود أو غير محذوف"));

            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Consultant.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, "تم استرجاع المستشار بنجاح"));
        }
        #endregion

        #region 11. Activate/Deactivate
        [HttpPut("Activate/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> Activate(long id)
        {
            var entity = await _unitOfWork.Consultant.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Consultant.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, "تم تفعيل المستشار بنجاح"));
        }

        [HttpPut("Deactivate/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> Deactivate(long id)
        {
            var entity = await _unitOfWork.Consultant.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Consultant.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultantDto>.Success(dto, "تم إلغاء تفعيل المستشار بنجاح"));
        }
        #endregion

        #region 12. RegisterRequest (Client Self-Registration)
        [HttpPost("RegisterRequest")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterRequest(
            [FromForm] ConsultantRequestCreateDto model,
            IFormFile? avatarFile,
            IFormFile? cvFile)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(ApiBaseResponse<string>.Fail("المستخدم غير مصادق"));

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return NotFound(ApiBaseResponse<string>.Fail("المستخدم غير موجود"));

            var existingConsultant = await _unitOfWork.Consultant.FindAsync(
                x => x.UserId == currentUserId && x.IsDeleted != true);

            if (existingConsultant != null)
                return BadRequest(ApiBaseResponse<string>.Fail("يوجد سجل مستشار مرتبط بهذا المستخدم بالفعل"));

            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Title) || !model.Gender.HasValue || model.Gender == Gender.Unknown)
                return BadRequest(ApiBaseResponse<string>.Fail("الاسم والمسمى والجنس حقول مطلوبة"));

            Consultant entity;
            try
            {
                entity = new Consultant
                {
                    Name = model.Name.Trim(),
                    Title = model.Title.Trim(),
                    Bio = model.Bio ?? string.Empty,
                    Gender = model.Gender.Value,
                    UserId = currentUserId,
                    YearsOfExperience = model.YearsOfExperience,
                    Specialty = model.Specialty,
                    HourlyRate = model.HourlyRate,
                    Availability = model.Availability,
                    LinkedInUrl = model.LinkedInUrl,
                    XUrl = model.XUrl,
                    InstagramUrl = model.InstagramUrl,
                    FacebookUrl = model.FacebookUrl,
                    AvatarUrl = SaveAvatarFile(avatarFile),
                    CvUrl = SaveCvFile(cvFile),
                    IsActive = false,
                    RequestStatus = InstructorRequestStatus.Pending,
                    SubmittedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId
                };
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiBaseResponse<string>.Fail(ex.Message));
            }

            await _unitOfWork.Consultant.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var skillNames = model.Skills?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (skillNames != null && skillNames.Any())
            {
                var skillEntities = skillNames.Select(skillName => new ConsultantSkill
                {
                    ConsultantId = entity.Id,
                    Name = skillName
                }).ToList();

                await _unitOfWork.ConsultantSkill.AddRangeAsync(skillEntities);
                await _unitOfWork.CompleteAsync();
            }

            // Save categories
            if (model.CategoryIds != null && model.CategoryIds.Any())
            {
                var categoryEntities = model.CategoryIds
                    .Distinct()
                    .Select(catId => new ConsultantConsultationCategory
                    {
                        ConsultantId = entity.Id,
                        ConsultationCategoryId = catId
                    }).ToList();

                await _unitOfWork.ConsultantConsultationCategory.AddRangeAsync(categoryEntities);
                await _unitOfWork.CompleteAsync();
            }

            entity = await _unitOfWork.Consultant.FindAsync(x => x.Id == entity.Id, ["User", "Skills", "ConsultantCategories.ConsultationCategory"]);

            return Ok(ApiBaseResponse<ConsultantDto>.Success(
                MapToDto(entity!),
                "تم تسجيل بيانات المستشار بنجاح وهي بانتظار المراجعة والاعتماد"));
        }
        #endregion

        #region Helper Methods
        private ConsultantDto MapToDto(Consultant entity)
        {
            return new ConsultantDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserEmail = entity.User?.Email,
                UserPhoneNumber = entity.User?.PhoneNumber,
                Name = entity.Name,
                Title = entity.Title,
                Bio = entity.Bio,
                AvatarUrl = BuildPublicUrl(entity.AvatarUrl),
                CvUrl = BuildPublicUrl(entity.CvUrl),
                Gender = entity.Gender,
                GenderName = entity.Gender.ToString(),
                Rating = entity.Rating,
                IsActive = entity.IsActive,
                YearsOfExperience = entity.YearsOfExperience,
                Specialty = entity.Specialty,
                HourlyRate = entity.HourlyRate,
                Availability = entity.Availability,
                LinkedInUrl = entity.LinkedInUrl,
                XUrl = entity.XUrl,
                InstagramUrl = entity.InstagramUrl,
                FacebookUrl = entity.FacebookUrl,
                SubmittedByUserId = entity.SubmittedByUserId,
                RequestStatus = entity.RequestStatus,
                RequestStatusName = entity.RequestStatus?.ToString(),
                ReviewedByUserId = entity.ReviewedByUserId,
                ReviewedAt = entity.ReviewedAt,
                DenialReason = entity.DenialReason,
                CreatedAt = entity.CreatedAt
            };
        }

        private ConsultantDetailDto MapToDetailDto(Consultant entity)
        {
            return new ConsultantDetailDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserEmail = entity.User?.Email,
                UserPhoneNumber = entity.User?.PhoneNumber,
                Name = entity.Name,
                Title = entity.Title,
                Bio = entity.Bio,
                AvatarUrl = BuildPublicUrl(entity.AvatarUrl),
                CvUrl = BuildPublicUrl(entity.CvUrl),
                Gender = entity.Gender,
                GenderName = entity.Gender.ToString(),
                Rating = entity.Rating,
                IsActive = entity.IsActive,
                YearsOfExperience = entity.YearsOfExperience,
                Specialty = entity.Specialty,
                HourlyRate = entity.HourlyRate,
                Availability = entity.Availability,
                LinkedInUrl = entity.LinkedInUrl,
                XUrl = entity.XUrl,
                InstagramUrl = entity.InstagramUrl,
                FacebookUrl = entity.FacebookUrl,
                SubmittedByUserId = entity.SubmittedByUserId,
                RequestStatus = entity.RequestStatus,
                RequestStatusName = entity.RequestStatus?.ToString(),
                ReviewedByUserId = entity.ReviewedByUserId,
                ReviewedAt = entity.ReviewedAt,
                DenialReason = entity.DenialReason,
                CreatedAt = entity.CreatedAt,
                Skills = entity.Skills?.Select(s => new ConsultantSkillDto
                {
                    Id = s.Id,
                    ConsultantId = s.ConsultantId,
                    Name = s.Name
                }).ToList() ?? new(),
                Categories = entity.ConsultantCategories?.Select(cc => new ConsultationCategoryDto
                {
                    Id = cc.ConsultationCategory?.Id ?? 0,
                    Name = cc.ConsultationCategory?.Name ?? string.Empty,
                    Description = cc.ConsultationCategory?.Description ?? string.Empty,
                    IsActive = cc.ConsultationCategory?.IsActive ?? false
                }).ToList() ?? new()
            };
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

        private string SaveAvatarFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("نوع ملف الصورة غير مسموح به");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "consultants");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return BuildPublicUrl($"/uploads/consultants/{fileName}") ?? string.Empty;
        }

        private string? SaveCvFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("نوع ملف السيرة الذاتية غير مسموح به");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("حجم ملف السيرة الذاتية يجب أن لا يتجاوز 5 ميجابايت");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "consultants", "cv");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return BuildPublicUrl($"/uploads/consultants/cv/{fileName}");
        }
        #endregion
    }
}
