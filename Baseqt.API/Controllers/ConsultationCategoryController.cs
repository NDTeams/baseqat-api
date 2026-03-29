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
    public class ConsultationCategoryController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public ConsultationCategoryController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 0. Get Active (Public)
        [HttpGet("GetActive")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await _unitOfWork.ConsultationCategory.FindAllAsync(
                criteria: x => x.IsActive && x.IsDeleted != true,
                includes: ["ConsultantCategories.Consultant"]);

            var dtos = result.Select(c => new ConsultationCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                ConsultantCount = c.ConsultantCategories?.Count(cc => cc.Consultant != null && cc.Consultant.IsActive && cc.Consultant.IsDeleted != true) ?? 0
            }).ToList();

            return Ok(ApiBaseResponse<List<ConsultationCategoryDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة أقسام الاستشارات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.ConsultationCategory.FindAllAsync(x => x.IsDeleted != true);

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<ConsultationCategoryDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة أقسام الاستشارات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] ConsultationCategoryFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<ConsultationCategory, bool>> criteria = x =>
                x.IsDeleted != true &&
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Name) || x.Name.Contains(filter.Name)) &&
                (filter.IsActive == null || x.IsActive == filter.IsActive.Value);

            var totalCount = await _unitOfWork.ConsultationCategory.CountAsync(criteria);

            var result = await _unitOfWork.ConsultationCategory.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<ConsultationCategoryDto>.Success(new List<ConsultationCategoryDto>(), ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<ConsultationCategoryDto>
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
        [isAllowed("إدارة أقسام الاستشارات", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.ConsultationCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<ConsultationCategoryDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة أقسام الاستشارات", "is_insert")]
        public async Task<IActionResult> Add(ConsultationCategoryCreateDto model)
        {
            var entity = new ConsultationCategory
            {
                Name = model.Name,
                Description = model.Description ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _unitOfWork.ConsultationCategory.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultationCategoryDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 5. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة أقسام الاستشارات", "is_update")]
        public async Task<IActionResult> Update(long id, ConsultationCategoryUpdateDto model)
        {
            var entity = await _unitOfWork.ConsultationCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Name)) entity.Name = model.Name;
            if (model.Description != null) entity.Description = model.Description;
            if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ConsultationCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultationCategoryDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 6. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة أقسام الاستشارات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.ConsultationCategory.GetByIdAsync(id);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.ConsultationCategory.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 7. Soft Delete
        [HttpPut("SoftDelete/{id}")]
        [isAllowed("إدارة أقسام الاستشارات", "is_delete")]
        public async Task<IActionResult> SoftDelete(long id)
        {
            var entity = await _unitOfWork.ConsultationCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ConsultationCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 8. Get Deleted
        [HttpGet("GetDeleted")]
        [isAllowed("إدارة أقسام الاستشارات", "is_displayed")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _unitOfWork.ConsultationCategory.FindAllAsync(x => x.IsDeleted == true);

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<ConsultationCategoryDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 9. Restore
        [HttpPut("Restore/{id}")]
        [isAllowed("إدارة أقسام الاستشارات", "is_update")]
        public async Task<IActionResult> Restore(long id)
        {
            var entity = await _unitOfWork.ConsultationCategory.FindAsync(x => x.Id == id && x.IsDeleted == true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail("القسم غير موجود أو غير محذوف"));

            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ConsultationCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultationCategoryDto>.Success(dto, "تم استرجاع القسم بنجاح"));
        }
        #endregion

        #region 10. Activate/Deactivate
        [HttpPut("Activate/{id}")]
        [isAllowed("إدارة أقسام الاستشارات", "is_update")]
        public async Task<IActionResult> Activate(long id)
        {
            var entity = await _unitOfWork.ConsultationCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ConsultationCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultationCategoryDto>.Success(dto, "تم تفعيل القسم بنجاح"));
        }

        [HttpPut("Deactivate/{id}")]
        [isAllowed("إدارة أقسام الاستشارات", "is_update")]
        public async Task<IActionResult> Deactivate(long id)
        {
            var entity = await _unitOfWork.ConsultationCategory.FindAsync(x => x.Id == id && x.IsDeleted != true);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ConsultationCategory.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultationCategoryDto>.Success(dto, "تم إلغاء تفعيل القسم بنجاح"));
        }
        #endregion

        #region Helper Methods
        private ConsultationCategoryDto MapToDto(ConsultationCategory entity)
        {
            return new ConsultationCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
        }
        #endregion
    }
}
