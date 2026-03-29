using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.CORE.Response.Pagination;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;
using Baseqt.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public CourseController(IDataUnit unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.Course.GetAllAsync(["CourseCategory", "Instructor"]);
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null,ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<Course, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Title) || x.Title.Contains(filter.Title)) &&
                (filter.Level == null || x.Level == filter.Level) &&
                (filter.Status == null || x.Status == filter.Status) &&
                (filter.CourseCategoryId == null || x.CourseCategoryId == filter.CourseCategoryId) &&
                (filter.InstructorId == null || x.InstructorId == filter.InstructorId) &&
                (filter.IsActive == null || x.IsActive == filter.IsActive) &&
                (filter.CourseType == null || x.CourseType == filter.CourseType) &&
                (filter.MinPrice == null || x.Price >= filter.MinPrice) &&
                (filter.MaxPrice == null || x.Price <= filter.MaxPrice) &&
                (filter.StartDateFrom == null || x.StartDate >= filter.StartDateFrom) &&
                (filter.StartDateTo == null || x.StartDate <= filter.StartDateTo);

            var totalCount = await _unitOfWork.Course.CountAsync(criteria);

            var result = await _unitOfWork.Course.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending,
                includes: ["CourseCategory", "Instructor"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseDto>.Success(null,ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseDto>
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
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.Course.FindAsync(x => x.Id == id, ["CourseCategory", "Instructor", "Sections"]);
            
            if (entity == null)
                return Ok(PagedResponse<CourseDto>.Success(null, ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<CourseDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة الدورات", "is_insert")]
        public async Task<IActionResult> Add(CourseCreateDto model)
        {
            var entity = new Course
            {
                Title = model.Title,
                Subtitle = model.Subtitle,
                Description = model.Description,
                Level = model.Level,
                Language = model.Language,
                HasCertificate = model.HasCertificate,
                Price = model.Price,
                CourseDays = model.CourseDays,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                DurationInDays = model.DurationInDays,
                TotalDurationInHours = model.TotalDurationInHours,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CourseType = model.CourseType,
                Location = model.Location,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                PlatformName = model.PlatformName,
                PlatformUrl = model.PlatformUrl,
                Status = model.Status,
                IsActive = model.IsActive,
                CourseCategoryId = model.CourseCategoryId,
                InstructorId = model.InstructorId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _unitOfWork.Course.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Load related entities for DTO
            var savedEntity = await _unitOfWork.Course.FindAsync(x => x.Id == entity.Id, ["CourseCategory", "Instructor"]);
            var dto = MapToDto(savedEntity!);
            return Ok(ApiBaseResponse<CourseDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 5. Upload Thumbnail Image
        [HttpPost("UploadThumbnail/{id}")]
        [isAllowed("إدارة الدورات", "is_update")]
        public async Task<IActionResult> UploadThumbnail(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = await _unitOfWork.Course.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiBaseResponse<string>.Fail("نوع الملف غير مسموح به"));

            // Create directory if not exists
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "courses", "thumbnails");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Delete old image if exists
            if (!string.IsNullOrEmpty(entity.ThumbnailUrl))
            {
                var oldImagePath = Path.Combine(_env.WebRootPath, entity.ThumbnailUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            // Save new image
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            entity.ThumbnailUrl = $"/uploads/courses/thumbnails/{fileName}";
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Course.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<object>.Success(new { ImageUrl = entity.ThumbnailUrl }, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة الدورات", "is_update")]
        public async Task<IActionResult> Update(long id, CourseUpdateDto model)
        {
            var entity = await _unitOfWork.Course.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Update fields if provided
            if (!string.IsNullOrEmpty(model.Title)) entity.Title = model.Title;
            if (model.Subtitle != null) entity.Subtitle = model.Subtitle;
            if (model.Description != null) entity.Description = model.Description;
            if (model.Level.HasValue) entity.Level = model.Level.Value;
            if (model.Language != null) entity.Language = model.Language;
            if (model.HasCertificate.HasValue) entity.HasCertificate = model.HasCertificate.Value;
            if (model.Price.HasValue) entity.Price = model.Price.Value;
            if (model.CourseDays.HasValue) entity.CourseDays = model.CourseDays.Value;
            if (model.StartTime.HasValue) entity.StartTime = model.StartTime.Value;
            if (model.EndTime.HasValue) entity.EndTime = model.EndTime.Value;
            if (model.DurationInDays.HasValue) entity.DurationInDays = model.DurationInDays.Value;
            if (model.TotalDurationInHours.HasValue) entity.TotalDurationInHours = model.TotalDurationInHours.Value;
            if (model.StartDate.HasValue) entity.StartDate = model.StartDate;
            if (model.EndDate.HasValue) entity.EndDate = model.EndDate;
            if (model.CourseType.HasValue) entity.CourseType = model.CourseType.Value;
            if (model.Location != null) entity.Location = model.Location;
            if (model.Latitude.HasValue) entity.Latitude = model.Latitude;
            if (model.Longitude.HasValue) entity.Longitude = model.Longitude;
            if (model.PlatformName != null) entity.PlatformName = model.PlatformName;
            if (model.PlatformUrl != null) entity.PlatformUrl = model.PlatformUrl;
            if (model.Status.HasValue) entity.Status = model.Status.Value;
            if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;
            if (model.CourseCategoryId.HasValue) entity.CourseCategoryId = model.CourseCategoryId.Value;
            if (model.InstructorId.HasValue) entity.InstructorId = model.InstructorId.Value;

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Course.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Load related entities for DTO
            var updatedEntity = await _unitOfWork.Course.FindAsync(x => x.Id == entity.Id, ["CourseCategory", "Instructor"]);
            var dto = MapToDto(updatedEntity!);
            return Ok(ApiBaseResponse<CourseDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 7. Update Thumbnail Image
        [HttpPut("UpdateThumbnail/{id}")]
        [isAllowed("إدارة الدورات", "is_update")]
        public async Task<IActionResult> UpdateThumbnail(long id, IFormFile file)
        {
            return await UploadThumbnail(id, file);
        }
        #endregion

        #region 8. Delete (Permanent)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة الدورات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.Course.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Delete thumbnail if exists
            if (!string.IsNullOrEmpty(entity.ThumbnailUrl))
            {
                var imagePath = Path.Combine(_env.WebRootPath, entity.ThumbnailUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _unitOfWork.Course.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 9. Soft Delete
        [HttpPut("SoftDelete/{id}")]
        [isAllowed("إدارة الدورات", "is_delete")]
        public async Task<IActionResult> SoftDelete(long id)
        {
            var entity = await _unitOfWork.Course.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Course.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 10. Get Deleted (Inactive)
        [HttpGet("GetDeleted")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _unitOfWork.Course.FindAllAsync(
                x => x.IsActive == false,
                ["CourseCategory", "Instructor"]
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 11. Restore Deleted
        [HttpPut("Restore/{id}")]
        [isAllowed("إدارة الدورات", "is_update")]
        public async Task<IActionResult> Restore(long id)
        {
            var entity = await _unitOfWork.Course.FindAsync(
                x => x.Id == id && x.IsActive == false,
                ["CourseCategory", "Instructor"]
            );

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail("الدورة غير موجودة أو غير محذوفة"));

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.Course.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseDto>.Success(dto, "تم استرجاع الدورة بنجاح"));
        }
        #endregion

        #region Additional: Get by Category
        [HttpGet("ByCategory/{categoryId}")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetByCategory(long categoryId)
        {
            var courses = await _unitOfWork.Course.FindAllAsync(
                x => x.CourseCategoryId == categoryId && x.IsActive,
                ["Instructor"]
            );

            var dtos = courses.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Additional: Get by Instructor
        [HttpGet("ByInstructor/{instructorId}")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetByInstructor(long instructorId)
        {
            var courses = await _unitOfWork.Course.FindAllAsync(
                x => x.InstructorId == instructorId && x.IsActive,
                ["CourseCategory"]
            );

            var dtos = courses.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Public: Get Active Courses
        [HttpGet("GetActive")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await _unitOfWork.Course.FindAllAsync(
                criteria: x => x.IsActive == true,
                includes: new[] { "CourseCategory", "Instructor" }
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Public: Get Active Course Detail by ID
        [HttpGet("GetActiveById/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveById(long id)
        {
            var entity = await _unitOfWork.Course.FindAsync(
                x => x.Id == id && x.IsActive == true,
                new[] { "CourseCategory", "Instructor", "Sections", "Sections.Lessons",
                        "CourseInstructors", "CourseInstructors.Instructor",
                        "Requirements", "Reviews", "Enrollments" }
            );

            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dto = MapToDetailDto(entity);
            return Ok(ApiBaseResponse<CourseDetailDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Public: Get Course Stats
        [HttpGet("GetCourseStats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseStats()
        {
            var totalCategories = await _unitOfWork.CourseCategory.CountAsync(x => x.IsActive);
            var totalCourses = await _unitOfWork.Course.CountAsync(x => x.IsActive);
            var totalEnrollments = await _unitOfWork.CourseEnrollment.CountAsync(x => true);

            var dto = new CourseStatsDto
            {
                TotalCategories = totalCategories,
                TotalCourses = totalCourses,
                TotalEnrollments = totalEnrollments
            };

            return Ok(ApiBaseResponse<CourseStatsDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private CourseDetailDto MapToDetailDto(Course entity)
        {
            var dto = new CourseDetailDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Subtitle = entity.Subtitle,
                Description = entity.Description,
                ThumbnailUrl = entity.ThumbnailUrl,
                PromoVideoUrl = entity.PromoVideoUrl,
                Level = entity.Level,
                LevelName = entity.Level.ToString(),
                Language = entity.Language,
                HasCertificate = entity.HasCertificate,
                Price = entity.Price,
                CourseDays = entity.CourseDays,
                CourseDaysName = entity.CourseDays.ToString(),
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                DurationInDays = entity.DurationInDays,
                TotalDurationInHours = entity.TotalDurationInHours,
                StartDate = DateHelper.ToHijri(entity.StartDate),
                EndDate = DateHelper.ToHijri(entity.EndDate),
                CourseType = entity.CourseType,
                CourseTypeName = entity.CourseType.ToString(),
                Location = entity.Location,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                PlatformName = entity.PlatformName,
                PlatformUrl = entity.PlatformUrl,
                Status = entity.Status,
                StatusName = entity.Status.ToString(),
                IsActive = entity.IsActive,
                CourseCategoryId = entity.CourseCategoryId,
                CourseCategoryName = entity.CourseCategory?.Name ?? string.Empty,
                InstructorId = entity.InstructorId,
                InstructorName = entity.Instructor?.Name ?? string.Empty,

                Sections = entity.Sections?.OrderBy(s => s.Order).Select(s => new CourseSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Order = s.Order,
                    Lessons = s.Lessons?.OrderBy(l => l.Order).Select(l => new CourseLessonDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        LessonType = l.LessonType,
                        LessonTypeName = l.LessonType.ToString(),
                        DurationInMinutes = l.DurationInMinutes,
                        IsPreview = l.IsPreview,
                        Order = l.Order,
                        CourseSectionId = l.CourseSectionId
                    }).ToList() ?? new List<CourseLessonDto>()
                }).ToList() ?? new List<CourseSectionDto>(),

                Instructors = entity.CourseInstructors?.Select(ci => new CourseInstructorDto
                {
                    CourseId = ci.CourseId,
                    InstructorId = ci.InstructorId,
                    InstructorName = ci.Instructor?.Name ?? string.Empty,
                    InstructorTitle = ci.Instructor?.Title ?? string.Empty,
                    InstructorAvatarUrl = ci.Instructor?.AvatarUrl
                }).ToList() ?? new List<CourseInstructorDto>(),

                Requirements = entity.Requirements?.OrderBy(r => r.Order).Select(r => new CourseRequirementDto
                {
                    Id = r.Id,
                    Text = r.Text,
                    Order = r.Order,
                    CourseId = r.CourseId
                }).ToList() ?? new List<CourseRequirementDto>(),

                Reviews = entity.Reviews?.Select(r => new CourseReviewDto
                {
                    Id = r.Id,
                    CourseId = r.CourseId,
                    CourseTitle = entity.Title,
                    UserId = r.UserId,
                    UserName = string.Empty,
                    Rating = r.Rating,
                    Comment = r.Comment
                }).ToList() ?? new List<CourseReviewDto>(),

                EnrollmentCount = entity.Enrollments?.Count ?? 0,
                AverageRating = entity.Reviews != null && entity.Reviews.Any()
                    ? Math.Round(entity.Reviews.Average(r => r.Rating), 1) : 0,
                TotalReviews = entity.Reviews?.Count ?? 0
            };

            return dto;
        }

        private CourseDto MapToDto(Course entity)
        {
            return new CourseDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Subtitle = entity.Subtitle,
                Description = entity.Description,
                ThumbnailUrl = entity.ThumbnailUrl,
                PromoVideoUrl = entity.PromoVideoUrl,
                Level = entity.Level,
                LevelName = entity.Level.ToString(),
                Language = entity.Language,
                HasCertificate = entity.HasCertificate,
                Price = entity.Price,
                CourseDays = entity.CourseDays,
                CourseDaysName = entity.CourseDays.ToString(),
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                DurationInDays = entity.DurationInDays,
                TotalDurationInHours = entity.TotalDurationInHours,
                StartDate = DateHelper.ToHijri(entity.StartDate),
                EndDate = DateHelper.ToHijri(entity.EndDate),
                CourseType = entity.CourseType,
                CourseTypeName = entity.CourseType.ToString(),
                Location = entity.Location,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                PlatformName = entity.PlatformName,
                PlatformUrl = entity.PlatformUrl,
                Status = entity.Status,
                StatusName = entity.Status.ToString(),
                IsActive = entity.IsActive,
                CourseCategoryId = entity.CourseCategoryId,
                CourseCategoryName = entity.CourseCategory?.Name ?? string.Empty,
                InstructorId = entity.InstructorId,
                InstructorName = entity.Instructor?.Name ?? string.Empty
            };
        }
        #endregion
    }
}
