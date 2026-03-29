using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class HomeStatisticController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public HomeStatisticController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.HomeStatistic.FindAllAsync(
                criteria: x => x.IsActive,
                skip: null, take: null,
                orderBy: x => x.SortOrder,
                orderByDirection: "ASC"
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(x => new HomeStatisticDto
            {
                Id = x.Id,
                Title = x.Title,
                Value = x.Value,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            }).ToList();

            return Ok(ApiBaseResponse<List<HomeStatisticDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var entity = await _unitOfWork.HomeStatistic.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = new HomeStatisticDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Value = entity.Value,
                Icon = entity.Icon,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            return Ok(ApiBaseResponse<HomeStatisticDto>.Success(dto, ResponseMessages.DataRetrieved));
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] HomeStatisticCreateDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Value))
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = new HomeStatistic
            {
                Title = model.Title.Trim(),
                Value = model.Value.Trim(),
                Icon = model.Icon?.Trim(),
                SortOrder = model.SortOrder,
                IsActive = true
            };

            await _unitOfWork.HomeStatistic.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = new HomeStatisticDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Value = entity.Value,
                Icon = entity.Icon,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            return Ok(ApiBaseResponse<HomeStatisticDto>.Success(dto, ResponseMessages.DataSaved));
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] HomeStatisticUpdateDto model)
        {
            if (model == null)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = await _unitOfWork.HomeStatistic.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrWhiteSpace(model.Title)) entity.Title = model.Title.Trim();
            if (!string.IsNullOrWhiteSpace(model.Value)) entity.Value = model.Value.Trim();
            if (model.Icon != null) entity.Icon = model.Icon.Trim();
            if (model.SortOrder.HasValue) entity.SortOrder = model.SortOrder.Value;
            if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;

            _unitOfWork.HomeStatistic.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = new HomeStatisticDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Value = entity.Value,
                Icon = entity.Icon,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            return Ok(ApiBaseResponse<HomeStatisticDto>.Success(dto, ResponseMessages.DataUpdated));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.HomeStatistic.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.HomeStatistic.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
    }
}
