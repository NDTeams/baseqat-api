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

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseReviewController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public CourseReviewController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.CourseReview.GetAllAsync(["Course", "User"]);
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<CourseReviewDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] CourseReviewFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<CourseReview, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (filter.CourseId == null || x.CourseId == filter.CourseId) &&
                (filter.UserId == null || x.UserId == filter.UserId) &&
                (filter.MinRating == null || x.Rating >= filter.MinRating) &&
                (filter.MaxRating == null || x.Rating <= filter.MaxRating);

            var totalCount = await _unitOfWork.CourseReview.CountAsync(criteria);

            var result = await _unitOfWork.CourseReview.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.CreatedAt,
                orderByDirection: OrderBy.Descending,
                includes: ["Course", "User"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<CourseReviewDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<CourseReviewDto>
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
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.CourseReview.FindAsync(x => x.Id == id, ["Course", "User"]);
            
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<CourseReviewDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة التقييمات", "is_insert")]
        public async Task<IActionResult> Add(CourseReviewCreateDto model)
        {
            // Check if user already reviewed this course
            var existing = await _unitOfWork.CourseReview.FindAsync(
                x => x.CourseId == model.CourseId && x.UserId == model.UserId);
            
            if (existing != null)
                return BadRequest(ApiBaseResponse<string>.Fail("المستخدم قيّم هذه الدورة بالفعل"));

            var entity = new CourseReview
            {
                CourseId = model.CourseId,
                UserId = model.UserId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CourseReview.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var savedEntity = await _unitOfWork.CourseReview.FindAsync(x => x.Id == entity.Id, ["Course", "User"]);
            var dto = MapToDto(savedEntity!);
            return Ok(ApiBaseResponse<CourseReviewDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة التقييمات", "is_update")]
        public async Task<IActionResult> Update(long id, CourseReviewUpdateDto model)
        {
            var entity = await _unitOfWork.CourseReview.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (model.Rating.HasValue) entity.Rating = model.Rating.Value;
            if (model.Comment != null) entity.Comment = model.Comment;

            _unitOfWork.CourseReview.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var savedEntity = await _unitOfWork.CourseReview.FindAsync(x => x.Id == entity.Id, ["Course", "User"]);
            var dto = MapToDto(savedEntity!);
            return Ok(ApiBaseResponse<CourseReviewDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 8. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة التقييمات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.CourseReview.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.CourseReview.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Additional: Get by Course
        [HttpGet("ByCourse/{courseId}")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetByCourse(long courseId)
        {
            var reviews = await _unitOfWork.CourseReview.FindAllAsync(
                x => x.CourseId == courseId,
                ["User"]
            );
            var dtos = reviews.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<CourseReviewDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Additional: Get Course Average Rating
        [HttpGet("AverageRating/{courseId}")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetAverageRating(long courseId)
        {
            var reviews = await _unitOfWork.CourseReview.FindAllAsync(x => x.CourseId == courseId);
            
            if (!reviews.Any())
                return Ok(ApiBaseResponse<object>.Success(new { AverageRating = 0, TotalReviews = 0 }, ResponseMessages.DataRetrieved));

            var averageRating = reviews.Average(r => r.Rating);
            var totalReviews = reviews.Count();

            return Ok(ApiBaseResponse<object>.Success(new { AverageRating = Math.Round(averageRating, 2), TotalReviews = totalReviews }, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private CourseReviewDto MapToDto(CourseReview entity)
        {
            return new CourseReviewDto
            {
                Id = entity.Id,
                CourseId = entity.CourseId,
                CourseTitle = entity.Course?.Title ?? string.Empty,
                UserId = entity.UserId,
                UserName = entity.User?.UserName ?? string.Empty,
                Rating = entity.Rating,
                Comment = entity.Comment
            };
        }
        #endregion
    }
}
