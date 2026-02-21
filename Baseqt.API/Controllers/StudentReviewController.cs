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
    public class StudentReviewController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public StudentReviewController(IDataUnit unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region 1. Get All
        [HttpGet("GetAll")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _unitOfWork.StudentReview.GetAllAsync(["Instructor", "Course", "User"]);
            
            if (result == null || !result.Any())
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(ApiBaseResponse<List<StudentReviewDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 2. Get All with Filters
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] StudentReviewFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<StudentReview, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (filter.InstructorId == null || x.InstructorId == filter.InstructorId) &&
                (filter.CourseId == null || x.CourseId == filter.CourseId) &&
                (string.IsNullOrEmpty(filter.UserId) || x.UserId == filter.UserId) &&
                (filter.MinRating == null || x.Rating >= filter.MinRating) &&
                (filter.MaxRating == null || x.Rating <= filter.MaxRating);

            var totalCount = await _unitOfWork.StudentReview.CountAsync(criteria);

            var result = await _unitOfWork.StudentReview.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.CreatedAt,
                orderByDirection: OrderBy.Descending,
                includes: ["Instructor", "Course", "User"]
            );

            if (result == null || !result.Any())
                return Ok(PagedResponse<StudentReviewDto>.Fail(ResponseMessages.NotFound));

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<StudentReviewDto>
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
            var entity = await _unitOfWork.StudentReview.FindAsync(x => x.Id == id, ["Instructor", "Course", "User"]);
            
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var dto = MapToDto(entity);

            return Ok(ApiBaseResponse<StudentReviewDto>.Success(dto, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Add
        [HttpPost("Add")]
        [isAllowed("إدارة التقييمات", "is_insert")]
        public async Task<IActionResult> Add(StudentReviewCreateDto model)
        {
            // Check if user already reviewed this instructor for this course
            var existing = await _unitOfWork.StudentReview.FindAsync(
                x => x.InstructorId == model.InstructorId && 
                     x.CourseId == model.CourseId && 
                     x.UserId == model.UserId);
            
            if (existing != null)
                return BadRequest(ApiBaseResponse<string>.Fail("المستخدم قيّم هذا المدرب لهذه الدورة بالفعل"));

            var entity = new StudentReview
            {
                InstructorId = model.InstructorId,
                CourseId = model.CourseId,
                UserId = model.UserId,
                Rating = model.Rating,
                Comment = model.Comment ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.StudentReview.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Update instructor's average rating
            await UpdateInstructorRating(model.InstructorId);

            var savedEntity = await _unitOfWork.StudentReview.FindAsync(x => x.Id == entity.Id, ["Instructor", "Course", "User"]);
            var dto = MapToDto(savedEntity!);
            return Ok(ApiBaseResponse<StudentReviewDto>.Success(dto, ResponseMessages.DataSaved));
        }
        #endregion

        #region 6. Update
        [HttpPut("Update/{id}")]
        [isAllowed("إدارة التقييمات", "is_update")]
        public async Task<IActionResult> Update(long id, StudentReviewUpdateDto model)
        {
            var entity = await _unitOfWork.StudentReview.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            if (model.Rating.HasValue) entity.Rating = model.Rating.Value;
            if (model.Comment != null) entity.Comment = model.Comment;

            _unitOfWork.StudentReview.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Update instructor's average rating
            await UpdateInstructorRating(entity.InstructorId);

            var savedEntity = await _unitOfWork.StudentReview.FindAsync(x => x.Id == entity.Id, ["Instructor", "Course", "User"]);
            var dto = MapToDto(savedEntity!);
            return Ok(ApiBaseResponse<StudentReviewDto>.Success(dto, ResponseMessages.DataUpdated));
        }
        #endregion

        #region 8. Delete
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة التقييمات", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.StudentReview.GetByIdAsync(id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var instructorId = entity.InstructorId;

            _unitOfWork.StudentReview.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            // Update instructor's average rating
            await UpdateInstructorRating(instructorId);

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region Additional: Get by Instructor
        [HttpGet("ByInstructor/{instructorId}")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetByInstructor(long instructorId)
        {
            var reviews = await _unitOfWork.StudentReview.FindAllAsync(
                x => x.InstructorId == instructorId,
                ["Course", "User"]
            );
            var dtos = reviews.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<StudentReviewDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Additional: Get by Course
        [HttpGet("ByCourse/{courseId}")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetByCourse(long courseId)
        {
            var reviews = await _unitOfWork.StudentReview.FindAllAsync(
                x => x.CourseId == courseId,
                ["Instructor", "User"]
            );
            var dtos = reviews.Select(MapToDto).ToList();
            return Ok(ApiBaseResponse<List<StudentReviewDto>>.Success(dtos, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Additional: Get Instructor Average Rating
        [HttpGet("InstructorAverage/{instructorId}")]
        [isAllowed("إدارة التقييمات", "is_displayed")]
        public async Task<IActionResult> GetInstructorAverageRating(long instructorId)
        {
            var reviews = await _unitOfWork.StudentReview.FindAllAsync(x => x.InstructorId == instructorId);
            
            if (!reviews.Any())
                return Ok(ApiBaseResponse<object>.Success(new { AverageRating = 0.0, TotalReviews = 0 }, ResponseMessages.DataRetrieved));

            var averageRating = reviews.Average(r => r.Rating);
            var totalReviews = reviews.Count();

            return Ok(ApiBaseResponse<object>.Success(new { AverageRating = Math.Round(averageRating, 2), TotalReviews = totalReviews }, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region Helper Methods
        private StudentReviewDto MapToDto(StudentReview entity)
        {
            return new StudentReviewDto
            {
                Id = entity.Id,
                InstructorId = entity.InstructorId,
                InstructorName = entity.Instructor?.Name ?? string.Empty,
                CourseId = entity.CourseId,
                CourseTitle = entity.Course?.Title ?? string.Empty,
                UserId = entity.UserId,
                UserName = entity.User?.UserName ?? string.Empty,
                Rating = entity.Rating,
                Comment = entity.Comment
            };
        }

        private async Task UpdateInstructorRating(long instructorId)
        {
            var instructor = await _unitOfWork.Instructor.GetByIdAsync(instructorId);
            if (instructor == null) return;

            var reviews = await _unitOfWork.StudentReview.FindAllAsync(x => x.InstructorId == instructorId);
            
            if (reviews.Any())
            {
                instructor.Rating = Math.Round(reviews.Average(r => r.Rating), 2);
            }
            else
            {
                instructor.Rating = null;
            }

            _unitOfWork.Instructor.Update(instructor);
            await _unitOfWork.CompleteAsync();
        }
        #endregion
    }
}
