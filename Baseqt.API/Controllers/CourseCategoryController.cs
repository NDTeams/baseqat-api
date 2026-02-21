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
    public class CourseCategoryController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseCategoryController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة التصنيفات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.CourseCategory.FindAllAsync(x => x.IsDeleted != true);

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null,ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseCategoryDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة التصنيفات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseCategoryFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<CourseCategory, bool>> criteria = x =>
                x.IsDeleted != true &&
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Name) || x.Name.Contains(filter.Name)) &&
                (filter.IsActive == null || x.IsActive == filter.IsActive.Value);

            var totalCount = await _unitOfWork.CourseCategory.CountAsync(criteria);

            var result = await _unitOfWork.CourseCategory.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseCategoryDto>.Success(new List<CourseCategoryDto>(), ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseCategoryDto>
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
        [isAllowed("إدارة التصنيفات", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.CourseCategory.FindAsync(x => x.Id == id && x.IsDeleted != true, ["Courses"]);

            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dto = new CourseCategoryDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CoursesCount = entity.Courses?.Count ?? 0,
                Courses = entity.Courses?.Select(c => new CourseSummaryDto
                {
                    Id = c.Id,
                    Title = c.Title
                }).ToList() ?? new()
            };

            return Ok(ApiBaseResponse<CourseCategoryDetailDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة التصنيفات", "is_insert")]
        public async Task<IActionResult> Add(CourseCategoryCreateDto model)
        {
            var entity = new CourseCategory
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _unitOfWork.CourseCategory.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseCategoryDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة التصنيفات", "is_update")]
        public async Task<IActionResult> Update(long id, CourseCategoryUpdateDto model)
        {
            var entity = await _unitOfWork.CourseCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Name)) entity.Name = model.Name;
            if (model.Description != null) entity.Description = model.Description;
            if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.CourseCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseCategoryDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 8. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة التصنيفات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.CourseCategory.GetByIdAsync(id);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            // Check if category has courses
            var hasCourses = await _unitOfWork.Course.FindAsync(x => x.CourseCategoryId == id && x.IsActive);
            if (hasCourses != null)
                return BadRequest(ApiBaseResponse<string>.Fail("لا يمكن حذف التصنيف لوجود دورات مرتبطة به"));

            _unitOfWork.CourseCategory.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 9. Soft Delete
        [HttpPut("SoftDelete/{id}")]
        [isAllowed("إدارة التصنيفات", "is_delete")]
        public async Task<IActionResult> SoftDelete(long id)
        {
            var entity = await _unitOfWork.CourseCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.CourseCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 10. Get Deleted
        [HttpGet("GetDeleted")]
        [isAllowed("إدارة التصنيفات", "is_displayed")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _unitOfWork.CourseCategory.FindAllAsync(x => x.IsDeleted == true);

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseCategoryDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 11. Restore Deleted
        [HttpPut("Restore/{id}")]
        [isAllowed("إدارة التصنيفات", "is_update")]
        public async Task<IActionResult> Restore(long id)
        {
            var entity = await _unitOfWork.CourseCategory.FindAsync(x => x.Id == id && x.IsDeleted == true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail("التصنيف غير موجود أو غير محذوف"));

            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.CourseCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseCategoryDto>.Success(dto, "تم استرجاع التصنيف بنجاح"));
        }
        #endregion

        #region 12. Activate/Deactivate
        [HttpPut("Activate/{id}")]
        [isAllowed("إدارة التصنيفات", "is_update")]
        public async Task<IActionResult> Activate(long id)
        {
            var entity = await _unitOfWork.CourseCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.CourseCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseCategoryDto>.Success(dto, "تم تفعيل التصنيف بنجاح"));
        }

        [HttpPut("Deactivate/{id}")]
        [isAllowed("إدارة التصنيفات", "is_update")]
        public async Task<IActionResult> Deactivate(long id)
        {
            var entity = await _unitOfWork.CourseCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.CourseCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseCategoryDto>.Success(dto, "تم إلغاء تفعيل التصنيف بنجاح"));
        }
        #endregion

        #region Additional: Get Active Categories
        [HttpGet("GetActive")]
        [isAllowed("إدارة التصنيفات", "is_displayed")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _unitOfWork.CourseCategory.FindAllAsync(x => x.IsActive && x.IsDeleted != true);

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseCategoryDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Home Page: Get Active Categories with Limited Fields
        [HttpGet("GetAllHome")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllHome()
        {
            var result = await _unitOfWork.CourseCategory.FindAllAsync(x => x.IsActive && x.IsDeleted != true);

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseCategoryDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private CourseCategoryDto MapToDto(CourseCategory entity)
        {
            return new CourseCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
        #endregion
    }
}

    