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
using System.Text.Json;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InstructorController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly UsersHelper _usersHelper;
        private readonly IConfiguration _configuration;

        public InstructorController(
            IDataUnit unitOfWork, 
            IWebHostEnvironment env, 
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            UsersHelper usersHelper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _env = env;
            _userManager = userManager;
            _emailService = emailService;
            _usersHelper = usersHelper;
            _configuration = configuration;
        }

        #region 0. Get Active (Public)
        [HttpGet("GetActive")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await _unitOfWork.Instructor.FindAllAsync(
                criteria: x => x.IsDeleted != true && x.IsActive == true,
                includes: ["Skills"]
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<InstructorDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }

        [HttpGet("GetActiveById/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveById(long id)
        {
            var entity = await _unitOfWork.Instructor.FindAsync(
                x => x.Id == id && x.IsDeleted != true && x.IsActive == true,
                ["Skills", "StudentReviews", "CourseInstructors", "CourseInstructors.Course"]
            );

            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dto = new InstructorDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Title = entity.Title,
                Bio = entity.Bio,
                AvatarUrl = BuildPublicUrl(entity.AvatarUrl),
                Gender = entity.Gender,
                GenderName = entity.Gender.ToString(),
                Rating = entity.Rating,
                TotalStudents = entity.TotalStudents,
                TotalCources = entity.TotalCources,
                IsActive = entity.IsActive,
                YearsOfExperience = entity.YearsOfExperience,
                LinkedInUrl = entity.LinkedInUrl,
                XUrl = entity.XUrl,
                InstagramUrl = entity.InstagramUrl,
                FacebookUrl = entity.FacebookUrl,
                CreatedAt = entity.CreatedAt,
                Skills = entity.Skills?.Select(s => new InstructorSkillDto
                {
                    Id = s.Id,
                    InstructorId = s.InstructorId,
                    Name = s.Name
                }).ToList() ?? new(),
                Courses = entity.CourseInstructors?.Where(ci => ci.Course != null).Select(ci => new CourseDto
                {
                    Id = ci.Course.Id,
                    Title = ci.Course.Title,
                    Subtitle = ci.Course.Subtitle,
                    Description = ci.Course.Description,
                    ThumbnailUrl = BuildPublicUrl(ci.Course.ThumbnailUrl),
                    Level = ci.Course.Level,
                    LevelName = ci.Course.Level.ToString(),
                    Price = ci.Course.Price,
                }).ToList() ?? new(),
                Reviews = entity.StudentReviews?.Select(r => new StudentReviewDto
                {
                    Id = r.Id,
                    InstructorId = r.InstructorId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                }).ToList() ?? new()
            };

            return Ok(ApiBaseResponse<InstructorDetailDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.Instructor.FindAllAsync(
                criteria: x => x.IsDeleted != true,
                includes: ["Skills", "User"]
            );
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<InstructorDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] InstructorFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<Instructor, bool>> criteria = x =>
                x.IsDeleted != true &&
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Name) || x.Name.Contains(filter.Name)) &&
                (string.IsNullOrEmpty(filter.Title) || x.Title.Contains(filter.Title)) &&
                (filter.Gender == null || x.Gender == filter.Gender) &&
                (filter.MinRating == null || x.Rating >= filter.MinRating) &&
                (filter.IsActive == null || x.IsActive == filter.IsActive);

            var totalCount = await _unitOfWork.Instructor.CountAsync(criteria);

            var result = await _unitOfWork.Instructor.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending,
                includes: ["Skills", "User"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<InstructorDto>.Success(null,ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<InstructorDto>
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
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.Instructor.FindAsync(x => x.Id == id, ["Skills", "StudentReviews", "CourseInstructors", "User"]);
            
            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null,ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة المدربين", "is_insert")]
        public async Task<IActionResult> Add(InstructorCreateDto model)
        {
            var entity = new Instructor
            {
                Name = model.Name,
                Title = model.Title,
                Bio = model.Bio ?? string.Empty,
                Gender = model.Gender,
                UserId = model.UserId,
                YearsOfExperience = model.YearsOfExperience,
                LinkedInUrl = model.LinkedInUrl,
                XUrl = model.XUrl,
                InstagramUrl = model.InstagramUrl,
                FacebookUrl = model.FacebookUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _unitOfWork.Instructor.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Load user if linked
            if (!string.IsNullOrEmpty(entity.UserId))
            {
                entity = await _unitOfWork.Instructor.FindAsync(x => x.Id == entity.Id, ["User"]);
            }

            var dto = MapToDto(entity!);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, ResponseMessages.DataSaved));
        }

        [HttpPost("AddWithAccount")]
        [isAllowed("إدارة المدربين", "is_insert")]
        public async Task<IActionResult> AddWithAccount(InstructorCreateWithAccountDto model)
        {
            // التحقق من عدم وجود بريد إلكتروني مكرر
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return Ok(ApiBaseResponse<string>.Fail("البريد الإلكتروني مستخدم بالفعل"));

            // إنشاء حساب المستخدم
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

            // إضافة صلاحية المدرب
            await _usersHelper.AddUserToRole(user.Id, "Instructor");

            // إنشاء سجل المدرب
            var instructor = new Instructor
            {
                Name = model.Name,
                Title = model.Title,
                Bio = model.Bio ?? string.Empty,
                Gender = model.Gender,
                UserId = user.Id,
                YearsOfExperience = model.YearsOfExperience,
                LinkedInUrl = model.LinkedInUrl,
                XUrl = model.XUrl,
                InstagramUrl = model.InstagramUrl,
                FacebookUrl = model.FacebookUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _unitOfWork.Instructor.AddAsync(instructor);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
            {
                // حذف المستخدم في حالة فشل إنشاء المدرب
                await _userManager.DeleteAsync(user);
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));
            }

            // تحميل بيانات المستخدم
            instructor = await _unitOfWork.Instructor.FindAsync(x => x.Id == instructor.Id, ["User"]);

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
            catch
            {
                // لا نفشل العملية إذا فشل إرسال البريد
            }

            var dto = MapToDto(instructor!);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 5. Upload Avatar Image
        [HttpPost("UploadAvatar/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> UploadAvatar(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiBaseResponse<string>.Fail("نوع الملف غير مسموح به"));

            // Create directory if not exists
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "instructors");
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

            entity.AvatarUrl = BuildPublicUrl($"/uploads/instructors/{fileName}");
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<object>.Success(new { AvatarUrl = BuildPublicUrl(entity.AvatarUrl) }, ResponseMessages.DataSaved));
        }
        #endregion

        #region 5.1 Upload CV
        [HttpPost("UploadCv/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> UploadCv(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
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
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "instructors", "cv");
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

            entity.CvUrl = BuildPublicUrl($"/uploads/instructors/cv/{fileName}");
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<object>.Success(new { CvUrl = BuildPublicUrl(entity.CvUrl) }, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> Update(long id, InstructorUpdateDto model)
        {
            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Update fields if provided
            if (!string.IsNullOrEmpty(model.Name)) entity.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Title)) entity.Title = model.Title;
            if (model.Bio != null) entity.Bio = model.Bio;
            if (model.Gender.HasValue) entity.Gender = model.Gender.Value;
            if (model.Rating.HasValue) entity.Rating = model.Rating.Value;
            if (model.TotalStudents.HasValue) entity.TotalStudents = model.TotalStudents.Value;
            if (model.TotalCources.HasValue) entity.TotalCources = model.TotalCources.Value;
            if (model.UserId != null) entity.UserId = string.IsNullOrEmpty(model.UserId) ? null : model.UserId;
            if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;
            if (model.YearsOfExperience.HasValue) entity.YearsOfExperience = model.YearsOfExperience;
            if (model.LinkedInUrl != null) entity.LinkedInUrl = model.LinkedInUrl;
            if (model.XUrl != null) entity.XUrl = model.XUrl;
            if (model.InstagramUrl != null) entity.InstagramUrl = model.InstagramUrl;
            if (model.FacebookUrl != null) entity.FacebookUrl = model.FacebookUrl;

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 7. Update Avatar Image
        [HttpPut("UpdateAvatar/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> UpdateAvatar(long id, IFormFile file)
        {
            return await UploadAvatar(id, file);
        }
        #endregion

        #region 8. Delete (Permanent)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة المدربين", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Delete avatar if exists
            if (!string.IsNullOrEmpty(entity.AvatarUrl))
            {
                var imagePath = GetPhysicalPath(entity.AvatarUrl);
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _unitOfWork.Instructor.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 9. Soft Delete
        [HttpPut("SoftDelete/{id}")]
        [isAllowed("إدارة المدربين", "is_delete")]
        public async Task<IActionResult> SoftDelete(long id)
        {
            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 10. Get Deleted
        [HttpGet("GetDeleted")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _unitOfWork.Instructor.FindAllAsync(x => x.IsDeleted == true);

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<InstructorDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 11. Restore Deleted
        [HttpPut("Restore/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> Restore(long id)
        {
            var entity = await _unitOfWork.Instructor.FindAsync(x => x.Id == id && x.IsDeleted == true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail("المدرب غير موجود أو غير محذوف"));

            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, "تم استرجاع المدرب بنجاح"));
        }
        #endregion

        #region 12. Activate/Deactivate
        [HttpPut("Activate/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> Activate(long id)
        {
            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Success(null,ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, "تم تفعيل المدرب بنجاح"));
        }

        [HttpPut("Deactivate/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> Deactivate(long id)
        {
            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, "تم إلغاء تفعيل المدرب بنجاح"));
        }
        #endregion

        #region 13. Link Instructor to User Account
        [HttpPut("LinkToUser/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> LinkToUser(long id, [FromQuery] string userId)
        {
            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return Ok(ApiBaseResponse<string>.Fail("المدرب غير موجود"));

            // Check if user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(ApiBaseResponse<string>.Fail("المستخدم غير موجود"));

            // Check if this user is already linked to another instructor
            var existingInstructor = await _unitOfWork.Instructor.FindAsync(x => x.UserId == userId && x.Id != id);
            if (existingInstructor != null)
                return BadRequest(ApiBaseResponse<string>.Fail("هذا المستخدم مرتبط بمدرب آخر"));

            entity.UserId = userId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Reload with User
            entity = await _unitOfWork.Instructor.FindAsync(x => x.Id == id, ["User"]);
            var dto = MapToDto(entity!);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, "تم ربط المدرب بالحساب بنجاح"));
        }
        #endregion

        #region 14. Unlink Instructor from User Account
        [HttpPut("UnlinkFromUser/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> UnlinkFromUser(long id)
        {
            var entity = await _unitOfWork.Instructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail("المدرب غير موجود"));

            if (string.IsNullOrEmpty(entity.UserId))
                return BadRequest(ApiBaseResponse<string>.Fail("المدرب غير مرتبط بأي حساب"));

            entity.UserId = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<InstructorDto>.Success(dto, "تم إلغاء ربط المدرب من الحساب بنجاح"));
        }
        #endregion

        #region Additional: Get Instructor's Courses
        [HttpGet("{id}/Courses")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetInstructorCourses(long id)
        {
            var courses = await _unitOfWork.Course.FindAllAsync(
                x => x.InstructorId == id && x.IsActive,
                ["CourseCategory"]
            );

            var dtos = courses.Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Subtitle = c.Subtitle,
                ThumbnailUrl = c.ThumbnailUrl,
                Level = c.Level,
                LevelName = c.Level.ToString(),
                Price = c.Price,
                Status = c.Status,
                StatusName = c.Status.ToString(),
                CourseCategoryId = c.CourseCategoryId,
                CourseCategoryName = c.CourseCategory?.Name ?? string.Empty
            }).ToList();

            return Ok(ApiBaseResponse<List<CourseDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Additional: Get Instructor's Reviews
        [HttpGet("{id}/Reviews")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetInstructorReviews(long id)
        {
            var reviews = await _unitOfWork.StudentReview.FindAllAsync(
                x => x.InstructorId == id,
                ["Course", "User"]
            );

            var dtos = reviews.Select(r => new StudentReviewDto
            {
                Id = r.Id,
                InstructorId = r.InstructorId,
                CourseId = r.CourseId,
                CourseTitle = r.Course?.Title ?? string.Empty,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? string.Empty,
                Rating = r.Rating,
                Comment = r.Comment
            }).ToList();

            return Ok(ApiBaseResponse<List<StudentReviewDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 15. Register Instructor Request
        [HttpPost("RegisterRequest")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterRequest(
            [FromForm] InstructorRequestCreateDto model,
            IFormFile? avatarFile,
            IFormFile? cvFile)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(ApiBaseResponse<string>.Fail("المستخدم غير مصادق"));

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return NotFound(ApiBaseResponse<string>.Fail("المستخدم غير موجود"));

            var existingInstructor = await _unitOfWork.Instructor.FindAsync(
                x => x.UserId == currentUserId && x.IsDeleted != true);

            if (existingInstructor != null)
                return BadRequest(ApiBaseResponse<string>.Fail("يوجد سجل مدرب مرتبط بهذا المستخدم بالفعل"));

            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Title) || !model.Gender.HasValue || model.Gender == Gender.Unknown)
                return BadRequest(ApiBaseResponse<string>.Fail("الاسم والمسمى والجنس حقول مطلوبة"));

            Instructor entity;
            try
            {
                entity = new Instructor
                {
                    Name = model.Name.Trim(),
                    Title = model.Title.Trim(),
                    Bio = model.Bio ?? string.Empty,
                    Gender = model.Gender.Value,
                    UserId = currentUserId,
                    YearsOfExperience = model.YearsOfExperience,
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

            await _unitOfWork.Instructor.AddAsync(entity);
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
                var skillEntities = skillNames.Select(skillName => new InstructorSkill
                {
                    InstructorId = entity.Id,
                    Name = skillName
                }).ToList();

                await _unitOfWork.InstructorSkill.AddRangeAsync(skillEntities);
                await _unitOfWork.CompleteAsync();
            }

            entity = await _unitOfWork.Instructor.FindAsync(x => x.Id == entity.Id, ["User", "Skills"]);

            return Ok(ApiBaseResponse<InstructorDto>.Success(
                MapToDto(entity!),
                "تم تسجيل بيانات المدرب بنجاح وهي بانتظار المراجعة والاعتماد"));
        }
        #endregion

        #region 16. Submit Instructor Request
        [HttpPost("SubmitRequest")]
        public async Task<IActionResult> SubmitRequest(InstructorRequestCreateDto model)
        {
            if (!model.InstructorId.HasValue || model.InstructorId.Value <= 0)
                return BadRequest(ApiBaseResponse<string>.Fail("رقم المدرب مطلوب"));

            var entity = await _unitOfWork.Instructor.GetByIdAsync(model.InstructorId.Value);
            if (entity == null || entity.IsDeleted == true)
                return NotFound(ApiBaseResponse<string>.Fail("المدرب غير موجود"));

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(ApiBaseResponse<string>.Fail("المستخدم غير مصادق"));

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var isBaseqatEmployee = currentUser != null &&
                (await _userManager.IsInRoleAsync(currentUser, IntitalRoles.BaseqatEmployee) ||
                 await _userManager.IsInRoleAsync(currentUser, IntitalRoles.Admin) ||
                 await _userManager.IsInRoleAsync(currentUser, IntitalRoles.SuperAdmin));

            if (!isBaseqatEmployee && entity.UserId != currentUserId)
                return Forbid();

            if (!HasAnyRequestedField(model))
                return BadRequest(ApiBaseResponse<string>.Fail("الطلب لا يحتوي أي بيانات للتحديث"));

            var payload = new InstructorRequestPayload
            {
                Name = model.Name,
                Title = model.Title,
                Bio = model.Bio,
                Gender = model.Gender,
                YearsOfExperience = model.YearsOfExperience,
                LinkedInUrl = model.LinkedInUrl,
                XUrl = model.XUrl,
                InstagramUrl = model.InstagramUrl,
                FacebookUrl = model.FacebookUrl
            };

            entity.PayloadJson = JsonSerializer.Serialize(payload);

            entity.RequestStatus = InstructorRequestStatus.Pending;
            entity.SubmittedByUserId = currentUserId;
            entity.ReviewedByUserId = null;
            entity.ReviewedAt = null;
            entity.DenialReason = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = currentUserId;

            _unitOfWork.Instructor.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<InstructorDto>.Success(
                MapRequestToDto(entity),
                "تم إرسال طلب تحديث بيانات المدرب بنجاح"));
        }
        #endregion

        #region 17. Get Instructor Requests
        [HttpGet("GetRequests")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetRequests(
            [FromQuery] long? instructorId,
            [FromQuery] InstructorRequestStatus? status)
        {
            Expression<Func<Instructor, bool>> criteria = x =>
                x.IsDeleted != true &&
                x.RequestStatus != null &&
                (instructorId == null || x.Id == instructorId) &&
                (status == null || x.RequestStatus == status);

            var result = await _unitOfWork.Instructor.FindAllAsync(
                criteria: criteria,
                skip: null,
                take: null,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending);

            var dtos = result.Select(MapRequestToDto).ToList();

            return Ok(ApiBaseResponse<List<InstructorDto>>.Success(
                dtos,
                dtos.Any() ? ResponseMessages.DataRetrieved : ResponseMessages.NotFound));
        }
        #endregion

        #region 18. Review Instructor Request
        [HttpPut("ReviewRequest/{requestId}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> ReviewRequest(long requestId, InstructorRequestReviewDto model)
        {
            var request = await _unitOfWork.Instructor.FindAsync(
                x => x.Id == requestId && x.IsDeleted != true);

            if (request == null)
                return NotFound(ApiBaseResponse<string>.Fail("طلب التحديث غير موجود"));

            if (request.RequestStatus != InstructorRequestStatus.Pending)
                return BadRequest(ApiBaseResponse<string>.Fail("تمت مراجعة هذا الطلب مسبقاً"));

            if (!model.Approve && string.IsNullOrWhiteSpace(model.DenialReason))
                return BadRequest(ApiBaseResponse<string>.Fail("يرجى إدخال سبب الرفض"));

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model.Approve)
            {
                // إذا كان هناك PayloadJson (طلب تحديث بيانات) نطبق التغييرات
                if (!string.IsNullOrWhiteSpace(request.PayloadJson))
                {
                    var payload = ParseRequestPayload(request.PayloadJson);
                    if (payload != null)
                    {
                        if (!string.IsNullOrWhiteSpace(payload.Name)) request.Name = payload.Name;
                        if (!string.IsNullOrWhiteSpace(payload.Title)) request.Title = payload.Title;
                        if (payload.Bio != null) request.Bio = payload.Bio;
                        if (payload.Gender.HasValue && payload.Gender.Value != Gender.Unknown) request.Gender = payload.Gender.Value;
                        if (payload.YearsOfExperience.HasValue) request.YearsOfExperience = payload.YearsOfExperience;
                        if (payload.LinkedInUrl != null) request.LinkedInUrl = payload.LinkedInUrl;
                        if (payload.XUrl != null) request.XUrl = payload.XUrl;
                        if (payload.InstagramUrl != null) request.InstagramUrl = payload.InstagramUrl;
                        if (payload.FacebookUrl != null) request.FacebookUrl = payload.FacebookUrl;
                    }
                }
                // طلب تسجيل جديد (RegisterRequest) - البيانات محفوظة مباشرة في الحقول

                request.RequestStatus = InstructorRequestStatus.Approved;
                request.DenialReason = null;
                request.PayloadJson = null;
                request.IsActive = true;
            }
            else
            {
                request.RequestStatus = InstructorRequestStatus.Denied;
                request.DenialReason = model.DenialReason?.Trim();
            }

            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = currentUserId;
            request.UpdatedAt = DateTime.UtcNow;
            request.UpdatedBy = currentUserId;

            _unitOfWork.Instructor.Update(request);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<InstructorDto>.Success(
                MapToDto(request),
                model.Approve ? "تم اعتماد طلب تحديث بيانات المدرب" : "تم رفض طلب تحديث بيانات المدرب"));
        }
        #endregion

        #region Helper Methods
        private InstructorDto MapToDto(Instructor entity)
        {
            return new InstructorDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserEmail = entity.User?.Email,
                Name = entity.Name,
                Title = entity.Title,
                Bio = entity.Bio,
                AvatarUrl = BuildPublicUrl(entity.AvatarUrl),
                CvUrl = BuildPublicUrl(entity.CvUrl),
                Gender = entity.Gender,
                GenderName = entity.Gender.ToString(),
                Rating = entity.Rating,
                TotalStudents = entity.TotalStudents,
                TotalCources = entity.TotalCources,
                IsActive = entity.IsActive,
                YearsOfExperience = entity.YearsOfExperience,
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

        private InstructorDto MapRequestToDto(Instructor entity)
        {
            var dto = MapToDto(entity);
            var payload = ParseRequestPayload(entity.PayloadJson);

            if (payload != null)
            {
                dto.Name = payload.Name ?? dto.Name;
                dto.Title = payload.Title ?? dto.Title;
                dto.Bio = payload.Bio ?? dto.Bio;
                dto.Gender = payload.Gender ?? dto.Gender;
                dto.GenderName = dto.Gender.ToString();
                dto.YearsOfExperience = payload.YearsOfExperience ?? dto.YearsOfExperience;
                dto.LinkedInUrl = payload.LinkedInUrl ?? dto.LinkedInUrl;
                dto.XUrl = payload.XUrl ?? dto.XUrl;
                dto.InstagramUrl = payload.InstagramUrl ?? dto.InstagramUrl;
                dto.FacebookUrl = payload.FacebookUrl ?? dto.FacebookUrl;
            }

            return dto;
        }

        private static InstructorRequestPayload? ParseRequestPayload(string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<InstructorRequestPayload>(payloadJson);
            }
            catch
            {
                return null;
            }
        }

        private class InstructorRequestPayload
        {
            public string? Name { get; set; }
            public string? Title { get; set; }
            public string? Bio { get; set; }
            public Gender? Gender { get; set; }
            public int? YearsOfExperience { get; set; }
            public string? LinkedInUrl { get; set; }
            public string? XUrl { get; set; }
            public string? InstagramUrl { get; set; }
            public string? FacebookUrl { get; set; }
        }

        private static bool HasAnyRequestedField(InstructorRequestCreateDto model)
        {
            return !string.IsNullOrWhiteSpace(model.Name) ||
                   !string.IsNullOrWhiteSpace(model.Title) ||
                   model.Bio != null ||
                   (model.Gender.HasValue && model.Gender.Value != Gender.Unknown) ||
                   model.YearsOfExperience.HasValue ||
                   model.LinkedInUrl != null ||
                   model.XUrl != null ||
                   model.InstagramUrl != null ||
                   model.FacebookUrl != null;
        }

        private string SaveAvatarFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("نوع ملف الصورة غير مسموح به");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "instructors");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return BuildPublicUrl($"/uploads/instructors/{fileName}") ?? string.Empty;
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

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "instructors", "cv");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return BuildPublicUrl($"/uploads/instructors/cv/{fileName}");
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
