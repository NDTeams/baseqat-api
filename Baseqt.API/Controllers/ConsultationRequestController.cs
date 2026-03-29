using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.CORE.Response.Pagination;
using Baseqat.CORE.Services;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models;
using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using Baseqt.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationRequestController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ConsultationRequestController(IDataUnit unitOfWork, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
        }

        #region 1. Submit (Authenticated Client)
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit(ConsultationRequestCreateDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("يجب تسجيل الدخول أولاً"));

            var category = await _unitOfWork.ConsultationCategory.FindAsync(
                x => x.Id == model.ConsultationCategoryId && x.IsActive && x.IsDeleted != true);
            if (category == null)
                return Ok(ApiBaseResponse<string>.Fail("نوع الاستشارة غير موجود أو غير مفعّل"));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(ApiBaseResponse<string>.Fail("المستخدم غير موجود"));

            var entity = new ConsultationRequest
            {
                ConsultationCategoryId = model.ConsultationCategoryId,
                UserId = userId,
                ClientName = user.FullName ?? user.UserName ?? string.Empty,
                ClientEmail = user.Email ?? string.Empty,
                ClientPhone = user.PhoneNumber ?? string.Empty,
                Subject = model.Subject,
                Message = model.Message,
                PreferredDate = model.PreferredDate,
                PreferredTime = model.PreferredTime,
                Status = ConsultationRequestStatus.PendingAssignment,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _unitOfWork.ConsultationRequest.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<ConsultationRequestDto>.Success(
                MapToDto(entity, null, category.Name), ResponseMessages.DataSaved));
        }
        #endregion

        #region 2. Get All with Filters (Admin)
        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] ConsultationRequestFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<ConsultationRequest, bool>> criteria = x =>
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.ClientName) || x.ClientName.Contains(filter.ClientName)) &&
                (filter.ConsultantId == null || filter.ConsultantId == 0 || x.ConsultantId == filter.ConsultantId) &&
                (filter.ConsultationCategoryId == null || filter.ConsultationCategoryId == 0 || x.ConsultationCategoryId == filter.ConsultationCategoryId) &&
                (filter.Status == null || x.Status == filter.Status.Value);

            var totalCount = await _unitOfWork.ConsultationRequest.CountAsync(criteria);

            var result = await _unitOfWork.ConsultationRequest.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            var dtos = new List<ConsultationRequestDto>();
            if (result != null && result.Any())
            {
                foreach (var entity in result)
                {
                    string? consultantName = null;
                    if (entity.ConsultantId.HasValue)
                    {
                        var consultant = await _unitOfWork.Consultant.GetByIdAsync(entity.ConsultantId.Value);
                        consultantName = consultant?.Name;
                    }
                    var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);
                    dtos.Add(MapToDto(entity, consultantName, category?.Name));
                }
            }

            return Ok(new PagedResponse<ConsultationRequestDto>
            {
                Data = dtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                Succeeded = true,
                Message = dtos.Any() ? ResponseMessages.DataRetrieved : ResponseMessages.NotFound
            });
        }
        #endregion

        #region 3. Get by ID (Admin)
        [HttpGet("GetById/{id}")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetById(long id)
        {
            var entity = await _unitOfWork.ConsultationRequest.FindAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            string? consultantName = null;
            if (entity.ConsultantId.HasValue)
            {
                var consultant = await _unitOfWork.Consultant.GetByIdAsync(entity.ConsultantId.Value);
                consultantName = consultant?.Name;
            }
            var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);

            return Ok(ApiBaseResponse<ConsultationRequestDto>.Success(
                MapToDto(entity, consultantName, category?.Name), ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 4. Update Status (Admin)
        [HttpPut("UpdateStatus/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> UpdateStatus(long id, ConsultationRequestStatusUpdateDto model)
        {
            var entity = await _unitOfWork.ConsultationRequest.FindAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.Status = model.Status;
            if (model.AdminNotes != null) entity.AdminNotes = model.AdminNotes;
            if (model.ZoomLink != null) entity.ZoomLink = model.ZoomLink;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ConsultationRequest.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            string? consultantName = null;
            if (entity.ConsultantId.HasValue)
            {
                var consultant = await _unitOfWork.Consultant.GetByIdAsync(entity.ConsultantId.Value);
                consultantName = consultant?.Name;
            }
            var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);

            // Send email when consultation is completed
            if (model.Status == ConsultationRequestStatus.Completed && !string.IsNullOrEmpty(entity.ClientEmail))
            {
                try
                {
                    var emailBody = EmailTemplates.GetConsultationCompletedEmail(
                        entity.ClientName, entity.Subject, consultantName ?? "غير محدد",
                        entity.SuggestedDate ?? entity.PreferredDate, entity.SuggestedTime ?? entity.PreferredTime);
                    await _emailService.SendEmailAsync(new List<string> { entity.ClientEmail }, "تم إكمال الاستشارة - باسقات", emailBody);
                }
                catch { /* Silent catch - don't fail the status update */ }
            }

            return Ok(ApiBaseResponse<ConsultationRequestDto>.Success(
                MapToDto(entity, consultantName, category?.Name), ResponseMessages.DataUpdated));
        }
        #endregion

        #region 5. Delete (Admin)
        [HttpDelete("Delete/{id}")]
        [isAllowed("إدارة المستشارين", "is_delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _unitOfWork.ConsultationRequest.FindAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            _unitOfWork.ConsultationRequest.Delete(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }
        #endregion

        #region 6. Assign Consultant (Admin)
        [HttpPut("AssignConsultant/{id}")]
        [isAllowed("إدارة المستشارين", "is_update")]
        public async Task<IActionResult> AssignConsultant(long id, ConsultationRequestAssignDto model)
        {
            var entity = await _unitOfWork.ConsultationRequest.FindAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            var consultant = await _unitOfWork.Consultant.FindAsync(
                x => x.Id == model.ConsultantId && x.IsDeleted != true && x.IsActive);
            if (consultant == null)
                return Ok(ApiBaseResponse<string>.Fail("المستشار غير موجود أو غير مفعّل"));

            // Check for scheduling conflicts if PreferredDate exists
            if (entity.PreferredDate.HasValue)
            {
                var conflicting = await _unitOfWork.ConsultationRequest.FindAllAsync(
                    criteria: x => x.ConsultantId == model.ConsultantId
                        && x.Id != id
                        && x.Status != ConsultationRequestStatus.Cancelled
                        && x.Status != ConsultationRequestStatus.Completed
                        && (
                            (x.SuggestedDate.HasValue && x.SuggestedDate.Value.Date == entity.PreferredDate.Value.Date) ||
                            (!x.SuggestedDate.HasValue && x.PreferredDate.HasValue && x.PreferredDate.Value.Date == entity.PreferredDate.Value.Date)
                        )
                );

                if (conflicting != null && conflicting.Any())
                {
                    // If time is specified, check time-level conflict
                    if (!string.IsNullOrEmpty(entity.PreferredTime))
                    {
                        var timeConflicts = conflicting.Where(x =>
                        {
                            var existingTime = x.SuggestedTime ?? x.PreferredTime;
                            return existingTime == entity.PreferredTime;
                        }).ToList();

                        if (timeConflicts.Any())
                        {
                            return Ok(ApiBaseResponse<string>.Fail(
                                $"يوجد تعارض في جدول المستشار: لديه {timeConflicts.Count} استشارة أخرى في نفس التاريخ والوقت"));
                        }
                    }
                    else
                    {
                        // No specific time - warn about same-day conflicts
                        return Ok(ApiBaseResponse<string>.Fail(
                            $"يوجد تعارض في جدول المستشار: لديه {conflicting.Count()} استشارة أخرى في نفس التاريخ"));
                    }
                }
            }

            entity.ConsultantId = model.ConsultantId;
            entity.Status = ConsultationRequestStatus.New;
            if (model.AdminNotes != null) entity.AdminNotes = model.AdminNotes;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ConsultationRequest.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);

            return Ok(ApiBaseResponse<ConsultationRequestDto>.Success(
                MapToDto(entity, consultant.Name, category?.Name), ResponseMessages.DataUpdated));
        }
        #endregion

        #region 7. Get My Assigned Requests (Consultant)
        [HttpGet("GetMyAssignedRequests")]
        public async Task<IActionResult> GetMyAssignedRequests(
            [FromQuery] PaginationParams pagination,
            [FromQuery] ConsultationRequestFilterDto filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("يجب تسجيل الدخول أولاً"));

            // Find consultant linked to current user
            var consultant = await _unitOfWork.Consultant.FindAsync(
                x => x.UserId == userId && x.IsDeleted != true);
            if (consultant == null)
                return Ok(new PagedResponse<ConsultationRequestDto>
                {
                    Data = new List<ConsultationRequestDto>(),
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalCount = 0,
                    Succeeded = true,
                    Message = "لا يوجد حساب مستشار مرتبط"
                });

            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<ConsultationRequest, bool>> criteria = x =>
                x.ConsultantId == consultant.Id &&
                (filter.Status == null || x.Status == filter.Status.Value) &&
                (filter.ConsultationCategoryId == null || filter.ConsultationCategoryId == 0 || x.ConsultationCategoryId == filter.ConsultationCategoryId) &&
                (string.IsNullOrEmpty(filter.ClientName) || x.ClientName.Contains(filter.ClientName));

            var totalCount = await _unitOfWork.ConsultationRequest.CountAsync(criteria);

            var result = await _unitOfWork.ConsultationRequest.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            var dtos = new List<ConsultationRequestDto>();
            if (result != null && result.Any())
            {
                foreach (var entity in result)
                {
                    var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);
                    dtos.Add(MapToDto(entity, consultant.Name, category?.Name));
                }
            }

            return Ok(new PagedResponse<ConsultationRequestDto>
            {
                Data = dtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                Succeeded = true,
                Message = dtos.Any() ? ResponseMessages.DataRetrieved : ResponseMessages.NotFound
            });
        }
        #endregion

        #region 8. Get My Requests (Authenticated Client)
        [HttpGet("GetMyRequests")]
        public async Task<IActionResult> GetMyRequests([FromQuery] PaginationParams pagination)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("يجب تسجيل الدخول أولاً"));

            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<ConsultationRequest, bool>> criteria = x => x.UserId == userId;

            var totalCount = await _unitOfWork.ConsultationRequest.CountAsync(criteria);

            var result = await _unitOfWork.ConsultationRequest.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.Id,
                orderByDirection: OrderBy.Descending
            );

            var dtos = new List<ConsultationRequestDto>();
            if (result != null && result.Any())
            {
                foreach (var entity in result)
                {
                    string? consultantName = null;
                    if (entity.ConsultantId.HasValue)
                    {
                        var consultant = await _unitOfWork.Consultant.GetByIdAsync(entity.ConsultantId.Value);
                        consultantName = consultant?.Name;
                    }
                    var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);
                    dtos.Add(MapToDto(entity, consultantName, category?.Name));
                }
            }

            return Ok(new PagedResponse<ConsultationRequestDto>
            {
                Data = dtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                Succeeded = true,
                Message = dtos.Any() ? ResponseMessages.DataRetrieved : ResponseMessages.NotFound
            });
        }
        #endregion

        #region 9. Consultant Response
        [HttpPut("RespondToRequest/{id}")]
        public async Task<IActionResult> RespondToRequest(long id, ConsultationRequestConsultantResponseDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("يجب تسجيل الدخول أولاً"));

            var consultant = await _unitOfWork.Consultant.FindAsync(
                x => x.UserId == userId && x.IsDeleted != true);
            if (consultant == null)
                return Ok(ApiBaseResponse<string>.Fail("لا يوجد حساب مستشار مرتبط"));

            var entity = await _unitOfWork.ConsultationRequest.FindAsync(
                x => x.Id == id && x.ConsultantId == consultant.Id);
            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.ConsultantResponse = model.ResponseType;
            entity.ConsultantNotes = model.ConsultantNotes;

            if (model.ResponseType == ConsultantResponseType.Approved)
            {
                entity.Status = ConsultationRequestStatus.Approved;
            }
            else if (model.ResponseType == ConsultantResponseType.Postponed)
            {
                entity.SuggestedDate = model.SuggestedDate;
                entity.SuggestedTime = model.SuggestedTime;
                entity.Status = ConsultationRequestStatus.InProgress;
            }
            else if (model.ResponseType == ConsultantResponseType.ChangeToInPerson)
            {
                entity.Status = ConsultationRequestStatus.InProgress;
            }

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            _unitOfWork.ConsultationRequest.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);

            return Ok(ApiBaseResponse<ConsultationRequestDto>.Success(
                MapToDto(entity, consultant.Name, category?.Name), ResponseMessages.DataUpdated));
        }
        #endregion

        #region 10. Get By Date Range (Admin Calendar)
        [HttpGet("GetByDateRange")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> GetByDateRange([FromQuery] ConsultationRequestCalendarQueryDto query)
        {
            Expression<Func<ConsultationRequest, bool>> criteria = x =>
                (
                    (x.SuggestedDate.HasValue && x.SuggestedDate.Value.Date >= query.StartDate.Date && x.SuggestedDate.Value.Date <= query.EndDate.Date) ||
                    (!x.SuggestedDate.HasValue && x.PreferredDate.HasValue && x.PreferredDate.Value.Date >= query.StartDate.Date && x.PreferredDate.Value.Date <= query.EndDate.Date)
                ) &&
                (query.ConsultantId == null || query.ConsultantId == 0 || x.ConsultantId == query.ConsultantId) &&
                (query.Status == null || x.Status == query.Status.Value);

            var result = await _unitOfWork.ConsultationRequest.FindAllAsync(criteria);

            var items = new List<ConsultationRequestCalendarItemDto>();
            if (result != null && result.Any())
            {
                foreach (var entity in result)
                {
                    string? consultantName = null;
                    if (entity.ConsultantId.HasValue)
                    {
                        var consultant = await _unitOfWork.Consultant.GetByIdAsync(entity.ConsultantId.Value);
                        consultantName = consultant?.Name;
                    }
                    var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);
                    items.Add(MapToCalendarItem(entity, consultantName, category?.Name));
                }
            }

            return Ok(ApiBaseResponse<List<ConsultationRequestCalendarItemDto>>.Success(items, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 11. Get My Requests By Date Range (Client Calendar)
        [HttpGet("GetMyRequestsByDateRange")]
        public async Task<IActionResult> GetMyRequestsByDateRange([FromQuery] ConsultationRequestCalendarQueryDto query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiBaseResponse<string>.Fail("يجب تسجيل الدخول أولاً"));

            Expression<Func<ConsultationRequest, bool>> criteria = x =>
                x.UserId == userId &&
                (
                    (x.SuggestedDate.HasValue && x.SuggestedDate.Value.Date >= query.StartDate.Date && x.SuggestedDate.Value.Date <= query.EndDate.Date) ||
                    (!x.SuggestedDate.HasValue && x.PreferredDate.HasValue && x.PreferredDate.Value.Date >= query.StartDate.Date && x.PreferredDate.Value.Date <= query.EndDate.Date)
                );

            var result = await _unitOfWork.ConsultationRequest.FindAllAsync(criteria);

            var items = new List<ConsultationRequestCalendarItemDto>();
            if (result != null && result.Any())
            {
                foreach (var entity in result)
                {
                    string? consultantName = null;
                    if (entity.ConsultantId.HasValue)
                    {
                        var consultant = await _unitOfWork.Consultant.GetByIdAsync(entity.ConsultantId.Value);
                        consultantName = consultant?.Name;
                    }
                    var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);
                    items.Add(MapToCalendarItem(entity, consultantName, category?.Name));
                }
            }

            return Ok(ApiBaseResponse<List<ConsultationRequestCalendarItemDto>>.Success(items, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 12. Get Public Calendar (No Auth)
        [HttpGet("GetPublicCalendar")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicCalendar([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            Expression<Func<ConsultationRequest, bool>> criteria = x =>
                (x.Status == ConsultationRequestStatus.Approved || x.Status == ConsultationRequestStatus.InProgress) &&
                (
                    (x.SuggestedDate.HasValue && x.SuggestedDate.Value.Date >= startDate.Date && x.SuggestedDate.Value.Date <= endDate.Date) ||
                    (!x.SuggestedDate.HasValue && x.PreferredDate.HasValue && x.PreferredDate.Value.Date >= startDate.Date && x.PreferredDate.Value.Date <= endDate.Date)
                );

            var result = await _unitOfWork.ConsultationRequest.FindAllAsync(criteria);

            var items = new List<ConsultationRequestCalendarItemDto>();
            if (result != null && result.Any())
            {
                foreach (var entity in result)
                {
                    string? consultantName = null;
                    if (entity.ConsultantId.HasValue)
                    {
                        var consultant = await _unitOfWork.Consultant.GetByIdAsync(entity.ConsultantId.Value);
                        consultantName = consultant?.Name;
                    }
                    var category = await _unitOfWork.ConsultationCategory.GetByIdAsync(entity.ConsultationCategoryId);
                    // Public calendar: limited info (no client details)
                    items.Add(new ConsultationRequestCalendarItemDto
                    {
                        Id = entity.Id,
                        Subject = entity.Subject,
                        ClientName = "", // Hidden for public
                        ConsultantName = consultantName,
                        ConsultantId = entity.ConsultantId,
                        ConsultationCategoryName = category?.Name,
                        PreferredDate = entity.PreferredDate,
                        PreferredTime = entity.PreferredTime,
                        SuggestedDate = entity.SuggestedDate,
                        SuggestedTime = entity.SuggestedTime,
                        Status = entity.Status,
                        StatusName = entity.Status.ToString()
                    });
                }
            }

            return Ok(ApiBaseResponse<List<ConsultationRequestCalendarItemDto>>.Success(items, ResponseMessages.DataRetrieved));
        }
        #endregion

        #region 13. Check Consultant Conflict (Admin)
        [HttpGet("CheckConsultantConflict")]
        [isAllowed("إدارة المستشارين", "is_displayed")]
        public async Task<IActionResult> CheckConsultantConflict(
            [FromQuery] long consultantId,
            [FromQuery] DateTime date,
            [FromQuery] string? time,
            [FromQuery] long? excludeRequestId)
        {
            var consultant = await _unitOfWork.Consultant.GetByIdAsync(consultantId);
            if (consultant == null)
                return Ok(ApiBaseResponse<string>.Fail("المستشار غير موجود"));

            Expression<Func<ConsultationRequest, bool>> criteria = x =>
                x.ConsultantId == consultantId &&
                (excludeRequestId == null || x.Id != excludeRequestId.Value) &&
                x.Status != ConsultationRequestStatus.Cancelled &&
                x.Status != ConsultationRequestStatus.Completed &&
                (
                    (x.SuggestedDate.HasValue && x.SuggestedDate.Value.Date == date.Date) ||
                    (!x.SuggestedDate.HasValue && x.PreferredDate.HasValue && x.PreferredDate.Value.Date == date.Date)
                );

            var conflicting = await _unitOfWork.ConsultationRequest.FindAllAsync(criteria: criteria);

            var filtered = conflicting?.ToList() ?? new List<ConsultationRequest>();

            // If time specified, filter by time too
            if (!string.IsNullOrEmpty(time))
            {
                filtered = filtered.Where(x =>
                {
                    var existingTime = x.SuggestedTime ?? x.PreferredTime;
                    return existingTime == time;
                }).ToList();
            }

            var items = new List<ConsultationRequestCalendarItemDto>();
            foreach (var c in filtered)
            {
                var cat = await _unitOfWork.ConsultationCategory.GetByIdAsync(c.ConsultationCategoryId);
                items.Add(MapToCalendarItem(c, consultant.Name, cat?.Name));
            }

            return Ok(ApiBaseResponse<ConsultantScheduleConflictDto>.Success(
                new ConsultantScheduleConflictDto
                {
                    HasConflict = items.Any(),
                    ConflictingRequests = items
                },
                items.Any() ? "يوجد تعارض في جدول المستشار" : "لا يوجد تعارض"
            ));
        }
        #endregion

        #region Helper Methods
        private static ConsultationRequestCalendarItemDto MapToCalendarItem(ConsultationRequest entity, string? consultantName, string? categoryName)
        {
            return new ConsultationRequestCalendarItemDto
            {
                Id = entity.Id,
                Subject = entity.Subject,
                ClientName = entity.ClientName,
                ConsultantName = consultantName,
                ConsultantId = entity.ConsultantId,
                ConsultationCategoryName = categoryName,
                PreferredDate = entity.PreferredDate,
                PreferredTime = entity.PreferredTime,
                SuggestedDate = entity.SuggestedDate,
                SuggestedTime = entity.SuggestedTime,
                Status = entity.Status,
                StatusName = entity.Status.ToString(),
                ZoomLink = entity.ZoomLink
            };
        }

        private static ConsultationRequestDto MapToDto(ConsultationRequest entity, string? consultantName, string? categoryName = null)
        {
            return new ConsultationRequestDto
            {
                Id = entity.Id,
                ConsultantId = entity.ConsultantId,
                ConsultantName = consultantName,
                ConsultationCategoryId = entity.ConsultationCategoryId,
                ConsultationCategoryName = categoryName,
                UserId = entity.UserId,
                ClientName = entity.ClientName,
                ClientEmail = entity.ClientEmail,
                ClientPhone = entity.ClientPhone,
                Subject = entity.Subject,
                Message = entity.Message,
                PreferredDate = entity.PreferredDate,
                PreferredTime = entity.PreferredTime,
                Status = entity.Status,
                StatusName = entity.Status.ToString(),
                AdminNotes = entity.AdminNotes,
                ConsultantResponse = entity.ConsultantResponse,
                ConsultantResponseName = entity.ConsultantResponse.ToString(),
                ConsultantNotes = entity.ConsultantNotes,
                SuggestedDate = entity.SuggestedDate,
                SuggestedTime = entity.SuggestedTime,
                ZoomLink = entity.ZoomLink,
                CreatedAt = entity.CreatedAt
            };
        }
        #endregion
    }
}
