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
    public class InstructorSkillController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public InstructorSkillController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var instructorId = await GetCurrentInstructorIdAsync();
            if (!instructorId.HasValue)
                return Ok(ApiBaseResponse<string>.Fail("لا يوجد سجل مدرب مرتبط بالمستخدم الحالي"));

            var result = await _unitOfWork.InstructorSkill.FindAllAsync(
                criteria: x => x.InstructorId == instructorId.Value,
                skip: null,
                take: null,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending);
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<InstructorSkillDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 1.1 Get All By InstructorId
        [HttpGet("ByInstructor/{instructorId}")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetByInstructorId(long instructorId)
        {
            var instructor = await _unitOfWork.Instructor.FindAsync(
                x => x.Id == instructorId && x.IsDeleted != true);

            if (instructor == null)
                return Ok(ApiBaseResponse<string>.Fail("المدرب غير موجود"));

            var result = await _unitOfWork.InstructorSkill.FindAllAsync(
                criteria: x => x.InstructorId == instructorId,
                skip: null,
                take: null,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending);

            var dtos = result?.Select(MapToDto).ToList() ?? new List<InstructorSkillDto>();
            return Ok(ApiBaseResponse<List<InstructorSkillDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة المدربين", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] InstructorSkillFilterDto filter)
        {
            var instructorId = await GetCurrentInstructorIdAsync();
            if (!instructorId.HasValue)
                return Ok(PagedResponse<InstructorSkillDto>.Fail("لا يوجد سجل مدرب مرتبط بالمستخدم الحالي"));

            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<InstructorSkill, bool>> criteria = x =>
                x.InstructorId == instructorId.Value &&
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Name) || x.Name.Contains(filter.Name));

            var totalCount = await _unitOfWork.InstructorSkill.CountAsync(criteria);

            var result = await _unitOfWork.InstructorSkill.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<InstructorSkillDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<InstructorSkillDto>
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
            var instructorId = await GetCurrentInstructorIdAsync();
            if (!instructorId.HasValue)
                return Ok(ApiBaseResponse<string>.Fail("لا يوجد سجل مدرب مرتبط بالمستخدم الحالي"));

            var entity = await _unitOfWork.InstructorSkill.GetByIdAsync(id);
            
            if (entity == null || entity.InstructorId != instructorId.Value)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<InstructorSkillDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة المدربين", "is_insert")]
        public async Task<IActionResult> Add(InstructorSkillCreateDto model)
        {
            long? instructorId = model.InstructorId;

            if (instructorId.HasValue)
            {
                var instructor = await _unitOfWork.Instructor.FindAsync(
                    x => x.Id == instructorId.Value && x.IsDeleted != true);

                if (instructor == null)
                    return Ok(ApiBaseResponse<string>.Fail("المدرب غير موجود"));
            }
            else
            {
                instructorId = await GetCurrentInstructorIdAsync();
                if (!instructorId.HasValue)
                    return Ok(ApiBaseResponse<string>.Fail("لا يوجد سجل مدرب مرتبط بالمستخدم الحالي"));
            }

            var entity = new InstructorSkill
            {
                Name = model.Name,
                InstructorId = instructorId.Value
            };

            await _unitOfWork.InstructorSkill.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<InstructorSkillDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة المدربين", "is_update")]
        public async Task<IActionResult> Update(long id, InstructorSkillUpdateDto model)
        {
            var entity = await _unitOfWork.InstructorSkill.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Name)) entity.Name = model.Name;

            _unitOfWork.InstructorSkill.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<InstructorSkillDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 8. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة المدربين", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.InstructorSkill.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.InstructorSkill.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Helper Methods
        private InstructorSkillDto MapToDto(InstructorSkill entity)
        {
            return new InstructorSkillDto
            {
                Id = entity.Id,
                InstructorId = entity.InstructorId,
                Name = entity.Name
            };
        }

        private async Task<long?> GetCurrentInstructorIdAsync()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return null;

            var instructor = await _unitOfWork.Instructor.FindAsync(
                x => x.UserId == currentUserId && x.IsDeleted != true);

            return instructor?.Id;
        }
        #endregion
    }
}
