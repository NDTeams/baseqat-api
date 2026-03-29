using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.CORE.Response.Pagination;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;
using Baseqat.EF.Models.Enums;
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
    public class MediaCenterController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public MediaCenterController(IDataUnit unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة المركز الإعلامي", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.MediaCenter.GetAllAsync();

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<MediaCenterDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة المركز الإعلامي", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] MediaCenterFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<MediaCenter, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.Title) || x.Title.Contains(filter.Title)) &&
                (filter.MediaType == null || x.MediaType == filter.MediaType) &&
                (string.IsNullOrEmpty(filter.Category) || x.Category == filter.Category) &&
                (filter.IsActive == null || x.IsActive == filter.IsActive);

            var totalCount = await _unitOfWork.MediaCenter.CountAsync(criteria);

            var result = await _unitOfWork.MediaCenter.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<MediaCenterDto>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<MediaCenterDto>
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
        [isAllowed("إدارة المركز الإعلامي", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.MediaCenter.FindAsync(x => x.Id == id);

            if (entity == null)
                return Ok(PagedResponse<MediaCenterDto>.Success(null, ResponseMessages.NotFound));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<MediaCenterDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة المركز الإعلامي", "is_insert")]
        public async Task<IActionResult> Add(MediaCenterCreateDto model)
        {
            var entity = new MediaCenter
            {
                Title = model.Title,
                Description = model.Description,
                MediaType = model.MediaType,
                Category = model.Category,
                Author = model.Author,
                Content = model.Content,
                ReadingTimeMinutes = model.ReadingTimeMinutes,
                EventDate = model.EventDate,
                EventEndDate = model.EventEndDate,
                EventTime = model.EventTime,
                EventLocation = model.EventLocation,
                VideoUrl = model.VideoUrl,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            await _unitOfWork.MediaCenter.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<MediaCenterDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 5. Upload Image
        [HttpPost("UploadImage/{id}")]
        [isAllowed("إدارة المركز الإعلامي", "is_update")]
        public async Task<IActionResult> UploadImage(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiBaseResponse<string>.Fail(ResponseMessages.InvalidData));

            var entity = await _unitOfWork.MediaCenter.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiBaseResponse<string>.Fail("نوع الملف غير مسموح به"));

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "media-center");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(entity.ImageUrl))
            {
                var oldImagePath = Path.Combine(_env.WebRootPath, entity.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            entity.ImageUrl = $"/uploads/media-center/{fileName}";
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.MediaCenter.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<object>.Success(new { ImageUrl = entity.ImageUrl }, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة المركز الإعلامي", "is_update")]
        public async Task<IActionResult> Update(long id, MediaCenterUpdateDto model)
        {
            var entity = await _unitOfWork.MediaCenter.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(model.Title)) entity.Title = model.Title;
            if (model.Description != null) entity.Description = model.Description;
            if (model.MediaType.HasValue) entity.MediaType = model.MediaType.Value;
            if (model.Category != null) entity.Category = model.Category;
            if (model.Author != null) entity.Author = model.Author;
            if (model.Content != null) entity.Content = model.Content;
            if (model.ReadingTimeMinutes.HasValue) entity.ReadingTimeMinutes = model.ReadingTimeMinutes;
            if (model.EventDate.HasValue) entity.EventDate = model.EventDate;
            if (model.EventEndDate.HasValue) entity.EventEndDate = model.EventEndDate;
            if (model.EventTime != null) entity.EventTime = model.EventTime;
            if (model.EventLocation != null) entity.EventLocation = model.EventLocation;
            if (model.VideoUrl != null) entity.VideoUrl = model.VideoUrl;
            if (model.IsActive.HasValue) entity.IsActive = model.IsActive.Value;

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.MediaCenter.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<MediaCenterDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 7. Delete (Permanent)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة المركز الإعلامي", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.MediaCenter.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (!string.IsNullOrEmpty(entity.ImageUrl))
            {
                var imagePath = Path.Combine(_env.WebRootPath, entity.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _unitOfWork.MediaCenter.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 8. Soft Delete
        [HttpPut("SoftDelete/{id}")]
        [isAllowed("إدارة المركز الإعلامي", "is_delete")]
        public async Task<IActionResult> SoftDelete(long id)
        {
            var entity = await _unitOfWork.MediaCenter.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.MediaCenter.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 9. Get Deleted (Inactive)
        [HttpGet("GetDeleted")]
        [isAllowed("إدارة المركز الإعلامي", "is_displayed")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _unitOfWork.MediaCenter.FindAllAsync(x => x.IsActive == false);

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<MediaCenterDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 10. Restore Deleted
        [HttpPut("Restore/{id}")]
        [isAllowed("إدارة المركز الإعلامي", "is_update")]
        public async Task<IActionResult> Restore(long id)
        {
            var entity = await _unitOfWork.MediaCenter.FindAsync(x => x.Id == id && x.IsActive == false);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail("العنصر غير موجود أو غير محذوف"));

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.MediaCenter.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<MediaCenterDto>.Success(dto, "تم استرجاع العنصر بنجاح"));
        }
        #endregion

        #region Public: Get Active
        [HttpGet("GetActive")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await _unitOfWork.MediaCenter.FindAllAsync(
                criteria: x => x.IsActive == true,
                skip: null,
                take: null,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<MediaCenterDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Public: Get Active by Type
        [HttpGet("GetActiveByType/{type}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveByType(MediaType type)
        {
            var result = await _unitOfWork.MediaCenter.FindAllAsync(
                criteria: x => x.IsActive == true && x.MediaType == type,
                skip: null,
                take: null,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<MediaCenterDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Public: Get Active by ID
        [HttpGet("GetActiveById/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveById(long id)
        {
            var entity = await _unitOfWork.MediaCenter.FindAsync(x => x.Id == id && x.IsActive == true);

            if (entity == null)
                return Ok(ApiBaseResponse<string>.Success(null, ResponseMessages.NotFound));

            var dto = MapToDto(entity);
            return Ok(ApiBaseResponse<MediaCenterDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private MediaCenterDto MapToDto(MediaCenter entity)
        {
            return new MediaCenterDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                MediaType = entity.MediaType,
                MediaTypeName = entity.MediaType.ToString(),
                Category = entity.Category,
                Author = entity.Author,
                Content = entity.Content,
                ReadingTimeMinutes = entity.ReadingTimeMinutes,
                EventDate = entity.EventDate,
                EventEndDate = entity.EventEndDate,
                EventTime = entity.EventTime,
                EventLocation = entity.EventLocation,
                VideoUrl = entity.VideoUrl,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
        }
        #endregion
    }
}
