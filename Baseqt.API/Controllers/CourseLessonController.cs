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
    public class CourseLessonController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseLessonController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.CourseLesson.GetAllAsync();
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseLessonDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseLessonFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<CourseLesson, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Title) || x.Title.Contains(filter.Title)) &&
                (filter.LessonType == null || x.LessonType == filter.LessonType) &&
                (filter.CourseSectionId == null || x.CourseSectionId == filter.CourseSectionId) &&
                (filter.IsPreview == null || x.IsPreview == filter.IsPreview);

            var totalCount = await _unitOfWork.CourseLesson.CountAsync(criteria);

            var result = await _unitOfWork.CourseLesson.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Order,
                orderByDirection: OrderBy.Ascending
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseLessonDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseLessonDto>
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
            var entity = await _unitOfWork.CourseLesson.GetByIdAsync(id);
            
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<CourseLessonDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة الدورات", "is_insert")]
        public async Task<IActionResult> Add(CourseLessonCreateDto model)
        {
            var entity = new CourseLesson
            {
                Title = model.Title,
                LessonType = model.LessonType,
                DurationInMinutes = model.DurationInMinutes,
                IsPreview = model.IsPreview,
                Order = model.Order,
                CourseSectionId = model.CourseSectionId
            };

            await _unitOfWork.CourseLesson.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseLessonDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة الدورات", "is_update")]
        public async Task<IActionResult> Update(long id, CourseLessonUpdateDto model)
        {
            var entity = await _unitOfWork.CourseLesson.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Title)) entity.Title = model.Title;
            if (model.LessonType.HasValue) entity.LessonType = model.LessonType.Value;
            if (model.DurationInMinutes.HasValue) entity.DurationInMinutes = model.DurationInMinutes.Value;
            if (model.IsPreview.HasValue) entity.IsPreview = model.IsPreview.Value;
            if (model.Order.HasValue) entity.Order = model.Order.Value;

            _unitOfWork.CourseLesson.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<CourseLessonDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 8. Delete (Permanent)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة الدورات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.CourseLesson.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.CourseLesson.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Additional: Get by Section
        [HttpGet("BySection/{sectionId}")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetBySection(long sectionId)
        {
            var lessons = await _unitOfWork.CourseLesson.FindAllAsync(x => x.CourseSectionId == sectionId);
            var dtos = lessons.OrderBy(l => l.Order).Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseLessonDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private CourseLessonDto MapToDto(CourseLesson entity)
        {
            return new CourseLessonDto
            {
                Id = entity.Id,
                Title = entity.Title,
                LessonType = entity.LessonType,
                LessonTypeName = entity.LessonType.ToString(),
                DurationInMinutes = entity.DurationInMinutes,
                IsPreview = entity.IsPreview,
                Order = entity.Order,
                CourseSectionId = entity.CourseSectionId
            };
        }
        #endregion
    }
}
