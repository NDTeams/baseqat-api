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
    public class CourseInstructorController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseInstructorController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.CourseInstructor.GetAllAsync(["Course", "Instructor"]);
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseInstructorDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseInstructorFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<CourseInstructor, bool>> criteria = x =>
                (filter.CourseId == null || x.CourseId == filter.CourseId) &&
                (filter.InstructorId == null || x.InstructorId == filter.InstructorId);

            var totalCount = await _unitOfWork.CourseInstructor.CountAsync(criteria);

            var result = await _unitOfWork.CourseInstructor.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending,
                includes: ["Course", "Instructor"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseInstructorDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseInstructorDto>
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
            var entity = await _unitOfWork.CourseInstructor.FindAsync(x => x.Id == id, ["Course", "Instructor"]);
            
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<CourseInstructorDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة الدورات", "is_insert")]
        public async Task<IActionResult> Add(CourseInstructorCreateDto model)
        {
            // Check if already assigned
            var existing = await _unitOfWork.CourseInstructor.FindAsync(
                x => x.CourseId == model.CourseId && x.InstructorId == model.InstructorId);
            
            if (existing != null)
                return BadRequest(ApiBaseResponse<string>.Fail("المدرب معين بالفعل لهذه الدورة"));

            var entity = new CourseInstructor
            {
                CourseId = model.CourseId,
                InstructorId = model.InstructorId
            };

            await _unitOfWork.CourseInstructor.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var savedEntity = await _unitOfWork.CourseInstructor.FindAsync(x => x.Id == entity.Id, ["Course", "Instructor"]);
            var dto = MapToDto(savedEntity!);
            return Ok(ApiBaseResponse<CourseInstructorDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 8. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة الدورات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.CourseInstructor.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.CourseInstructor.Delete(entity);
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
            var items = await _unitOfWork.CourseInstructor.FindAllAsync(
                x => x.CourseId == courseId,
                ["Instructor"]
            );
            var dtos = items.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseInstructorDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Additional: Get by Instructor
        [HttpGet("ByInstructor/{instructorId}")]
        [isAllowed("إدارة الدورات", "is_displayed")]
        public async Task<IActionResult> GetByInstructor(long instructorId)
        {
            var items = await _unitOfWork.CourseInstructor.FindAllAsync(
                x => x.InstructorId == instructorId,
                ["Course"]
            );
            var dtos = items.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseInstructorDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private CourseInstructorDto MapToDto(CourseInstructor entity)
        {
            return new CourseInstructorDto
            {
                Id = entity.Id,
                CourseId = entity.CourseId,
                CourseTitle = entity.Course?.Title ?? string.Empty,
                InstructorId = entity.InstructorId,
                InstructorName = entity.Instructor?.Name ?? string.Empty,
                InstructorTitle = entity.Instructor?.Title ?? string.Empty,
                InstructorAvatarUrl = entity.Instructor?.AvatarUrl
            };
        }
        #endregion
    }
}
