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
    public class ConsultantSkillController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public ConsultantSkillController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All By ConsultantId
        [HttpGet("GetByConsultantId/{consultantId}")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetByConsultantId(long consultantId)
        {
            var consultant = await _unitOfWork.Consultant.FindAsync(
                x => x.Id == consultantId && x.IsDeleted != true);

            if (consultant == null)
                return Ok(ApiBaseResponse<string>.Fail("المستشار غير موجود"));

            var result = await _unitOfWork.ConsultantSkill.FindAllAsync(
                criteria: x => x.ConsultantId == consultantId,
                skip: null,
                take: null,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending);

            var dtos = result?.Select(MapToDto).ToList() ?? new List<ConsultantSkillDto>();
            return Ok(ApiBaseResponse<List<ConsultantSkillDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] ConsultantSkillFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<ConsultantSkill, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (filter.ConsultantId == null || filter.ConsultantId == 0 || x.ConsultantId == filter.ConsultantId) &&
                (string.IsNullOrEmpty(filter.Name) || x.Name.Contains(filter.Name));

            var totalCount = await _unitOfWork.ConsultantSkill.CountAsync(criteria);

            var result = await _unitOfWork.ConsultantSkill.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<ConsultantSkillDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<ConsultantSkillDto>
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
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.ConsultantSkill.GetByIdAsync(id);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<ConsultantSkillDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة المستشارين", "is_insert")]
        public async Task<IActionResult> Add(ConsultantSkillCreateDto model)
        {
            long? consultantId = model.ConsultantId;

            if (consultantId.HasValue)
            {
                var consultant = await _unitOfWork.Consultant.FindAsync(
                    x => x.Id == consultantId.Value && x.IsDeleted != true);

                if (consultant == null)
                    return Ok(ApiBaseResponse<string>.Fail("المستشار غير موجود"));
            }
            else
            {
                return Ok(ApiBaseResponse<string>.Fail("يجب تحديد المستشار"));
            }

            var entity = new ConsultantSkill
            {
                Name = model.Name,
                ConsultantId = consultantId.Value
            };

            await _unitOfWork.ConsultantSkill.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultantSkillDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 5. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> Update(long id, ConsultantSkillUpdateDto model)
        {
            var entity = await _unitOfWork.ConsultantSkill.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Name)) entity.Name = model.Name;

            _unitOfWork.ConsultantSkill.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<ConsultantSkillDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 6. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة المستشارين", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.ConsultantSkill.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.ConsultantSkill.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Helper Methods
        private ConsultantSkillDto MapToDto(ConsultantSkill entity)
        {
            return new ConsultantSkillDto
            {
                Id = entity.Id,
                ConsultantId = entity.ConsultantId,
                Name = entity.Name
            };
        }
        #endregion
    }
}
