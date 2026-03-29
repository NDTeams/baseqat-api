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
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseEnrollmentController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseEnrollmentController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة التسجيلات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.CourseEnrollment.GetAllAsync(["Course", "User"]);
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseEnrollmentDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة التسجيلات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseEnrollmentFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<CourseEnrollment, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (filter.CourseId == null || x.CourseId == filter.CourseId) &&
                (string.IsNullOrEmpty(filter.UserId) || x.UserId == filter.UserId) &&
                (filter.EnrolledFrom == null || x.EnrolledAt >= filter.EnrolledFrom) &&
                (filter.EnrolledTo == null || x.EnrolledAt <= filter.EnrolledTo);

            var totalCount = await _unitOfWork.CourseEnrollment.CountAsync(criteria);

            var result = await _unitOfWork.CourseEnrollment.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.EnrolledAt,
                orderByDirection: OrderBy.Descending,
                includes: ["Course", "User"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseEnrollmentDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseEnrollmentDto>
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
        [isAllowed("إدارة التسجيلات", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.CourseEnrollment.FindAsync(x => x.Id == id, ["Course", "User"]);
            
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<CourseEnrollmentDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add (Enroll)
        [HttpPost("Add")]
        [isAllowed("إدارة التسجيلات", "is_insert")]
        public async Task<IActionResult> Add(CourseEnrollmentCreateDto model)
        {
            // Check if already enrolled
            var existing = await _unitOfWork.CourseEnrollment.FindAsync(
                x => x.CourseId == model.CourseId && x.UserId == model.UserId);
            
            if (existing != null)
                return BadRequest(ApiBaseResponse<string>.Fail("المستخدم مسجل بالفعل في هذه الدورة"));

            var entity = new CourseEnrollment
            {
                CourseId = model.CourseId,
                UserId = model.UserId,
                EnrolledAt = DateTime.UtcNow
            };

            await _unitOfWork.CourseEnrollment.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var savedEntity = await _unitOfWork.CourseEnrollment.FindAsync(x => x.Id == entity.Id, ["Course", "User"]);
            var dto = MapToDto(savedEntity!);
            return Ok(ApiBaseResponse<CourseEnrollmentDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 8. Delete (Unenroll)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة التسجيلات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.CourseEnrollment.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.CourseEnrollment.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Additional: Get by Course
        [HttpGet("ByCourse/{courseId}")]
        [isAllowed("إدارة التسجيلات", "is_displayed")]
        public async Task<IActionResult> GetByCourse(long courseId)
        {
            var enrollments = await _unitOfWork.CourseEnrollment.FindAllAsync(
                x => x.CourseId == courseId,
                ["User"]
            );
            var dtos = enrollments.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseEnrollmentDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Additional: Get by User
        [HttpGet("ByUser/{userId}")]
        [isAllowed("إدارة التسجيلات", "is_displayed")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var enrollments = await _unitOfWork.CourseEnrollment.FindAllAsync(
                x => x.UserId == userId,
                ["Course"]
            );
            var dtos = enrollments.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseEnrollmentDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Client: Get My Enrollments
        [HttpGet("GetMyEnrollments")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("غير مصرح"));

            var enrollments = await _unitOfWork.CourseEnrollment.FindAllAsync(
                x => x.UserId == userId,
                ["Course", "Course.Instructor", "Course.Sections", "Course.Enrollments"]
            );

            var dtos = enrollments.Select(MapToMyEnrollmentDto).ToList();
            return Ok(ApiBaseResponse<List<MyEnrollmentDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");
        }

        private CourseEnrollmentDto MapToDto(CourseEnrollment entity)
        {
            return new CourseEnrollmentDto
            {
                Id = entity.Id,
                CourseId = entity.CourseId,
                CourseTitle = entity.Course?.Title ?? string.Empty,
                UserId = entity.UserId,
                UserName = entity.User?.UserName ?? string.Empty,
                UserEmail = entity.User?.Email ?? string.Empty,
                EnrolledAt = DateHelper.ToHijri(entity.EnrolledAt)
            };
        }

        private MyEnrollmentDto MapToMyEnrollmentDto(CourseEnrollment entity)
        {
            var course = entity.Course;
            return new MyEnrollmentDto
            {
                EnrollmentId = entity.Id,
                EnrolledAt = DateHelper.ToHijri(entity.EnrolledAt),
                CourseId = entity.CourseId,
                CourseTitle = course?.Title ?? string.Empty,
                CourseSubtitle = course?.Subtitle,
                ThumbnailUrl = course?.ThumbnailUrl,
                Level = course?.Level ?? 0,
                LevelName = course?.Level.ToString() ?? string.Empty,
                CourseType = course?.CourseType ?? 0,
                CourseTypeName = course?.CourseType.ToString() ?? string.Empty,
                Status = course?.Status ?? 0,
                StatusName = course?.Status.ToString() ?? string.Empty,
                TotalDurationInHours = course?.TotalDurationInHours ?? 0,
                HasCertificate = course?.HasCertificate ?? false,
                StartDate = DateHelper.ToHijri(course?.StartDate),
                EndDate = DateHelper.ToHijri(course?.EndDate),
                Location = course?.Location,
                InstructorName = course?.Instructor?.Name ?? string.Empty,
                InstructorAvatarUrl = course?.Instructor?.AvatarUrl,
                TotalSections = course?.Sections?.Count ?? 0,
                TotalEnrollments = course?.Enrollments?.Count ?? 0,
            };
        }
        #endregion
    }
}
