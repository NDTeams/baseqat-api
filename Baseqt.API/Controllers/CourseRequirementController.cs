using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response;
using Baseqat.CORE.Response.Pagination;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;
using Baseqt.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseRequirementController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseRequirementController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.CourseRequirement.GetAllAsync();
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseRequirementDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseRequirementFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<CourseRequirement, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Text) || x.Text.Contains(filter.Text)) &&
                (filter.CourseId == null || x.CourseId == filter.CourseId);

            var totalCount = await _unitOfWork.CourseRequirement.CountAsync(criteria);

            var result = await _unitOfWork.CourseRequirement.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Order,
                orderByDirection: OrderBy.Ascending
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseRequirementDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseRequirementDto>
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
            var entity = await _unitOfWork.CourseRequirement.GetByIdAsync(id);
            
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<CourseRequirementDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة الدورات", "is_insert")]
        public async Task<IActionResult> Add(CourseRequirementCreateDto model)
        {
            var entity = new CourseRequirement
            {
                Text = model.Text,
                Order = model.Order,
                CourseId = model.CourseId
            };

            await _unitOfWork.CourseRequirement.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseRequirementDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة الدورات", "is_update")]
        public async Task<IActionResult> Update(long id, CourseRequirementUpdateDto model)
        {
            var entity = await _unitOfWork.CourseRequirement.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Text)) entity.Text = model.Text;
            if (model.Order.HasValue) entity.Order = model.Order.Value;

            _unitOfWork.CourseRequirement.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseRequirementDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 8. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة الدورات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.CourseRequirement.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.CourseRequirement.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Additional: Get by Course
        [HttpGet("ByCourse/{courseId}")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetByCourse(long courseId)
        {
            var requirements = await _unitOfWork.CourseRequirement.FindAllAsync(x => x.CourseId == courseId);
            var dtos = requirements.OrderBy(r => r.Order).Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseRequirementDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private CourseRequirementDto MapToDto(CourseRequirement entity)
        {
            return new CourseRequirementDto
            {
                Id = entity.Id,
                Text = entity.Text,
                Order = entity.Order,
                CourseId = entity.CourseId
            };
        }
        #endregion
    }
}
