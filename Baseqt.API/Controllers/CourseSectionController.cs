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
using System.Security.Claims;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseSectionController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseSectionController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.CourseSection.GetAllAsync(["Lessons"]);
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseSectionDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseSectionFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<CourseSection, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Title) || x.Title.Contains(filter.Title)) &&
                (filter.CourseId == null || filter.CourseId == 0 || x.CourseId == filter.CourseId);

            var totalCount = await _unitOfWork.CourseSection.CountAsync(criteria);

            var result = await _unitOfWork.CourseSection.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Order,
                orderByDirection: OrderBy.Ascending,
                includes: ["Lessons"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseSectionDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseSectionDto>
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
            var entity = await _unitOfWork.CourseSection.FindAsync(x => x.Id == id, ["Lessons"]);
            
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<CourseSectionDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة الدورات", "is_insert")]
        public async Task<IActionResult> Add(CourseSectionCreateDto model)
        {
            var entity = new CourseSection
            {
                Title = model.Title,
                Order = model.Order,
                CourseId = model.CourseId
            };

            await _unitOfWork.CourseSection.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseSectionDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة الدورات", "is_update")]
        public async Task<IActionResult> Update(long id, CourseSectionUpdateDto model)
        {
            var entity = await _unitOfWork.CourseSection.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Title)) entity.Title = model.Title;
            if (model.Order.HasValue) entity.Order = model.Order.Value;

            _unitOfWork.CourseSection.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseSectionDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 8. Delete (Permanent)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة الدورات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.CourseSection.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.CourseSection.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Helper Methods
        private CourseSectionDto MapToDto(CourseSection entity)
        {
            return new CourseSectionDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Order = entity.Order,
                Lessons = entity.Lessons?.Select(l => new CourseLessonDto
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
            };
        }
        #endregion
    }
}
