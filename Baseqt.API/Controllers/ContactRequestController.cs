using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response;
using Baseqat.CORE.Response.Pagination;
using Baseqat.CORE.Services;
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
    
    public class ContactRequestController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;
        private readonly IEmailService _emailService;

        public ContactRequestController(IDataUnit unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        [HttpPost("Send")]
        [AllowAnonymous]
        public async Task<IActionResult> Send(ContactRequestCreateDto model)
        {
            var entity = new ContactRequest
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                RequestType = string.IsNullOrWhiteSpace(model.RequestType) ? "استفسار عام" : model.RequestType,
                Message = model.Message,
                PreferredReplyChannel = model.PreferredReplyChannel,
                Status = ContactRequestStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ContactRequest.AddAsync(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<ContactRequestDto>.Success(MapToDto(entity), ResponseMessages.DataSaved));
        }

        [HttpGet("GetAllAsync")]
        [isAllowed("إدارة الطلبات", "is_displayed")]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationParams pagination, [FromQuery] ContactRequestFilterDto filter)
        {
            int skip = (pagination.PageNumber - 1) * pagination.PageSize;

            Expression<Func<ContactRequest, bool>> criteria = x =>
                x.IsDeleted != true &&
                (filter.Id == null || filter.Id == 0 || x.Id == filter.Id) &&
                (string.IsNullOrEmpty(filter.FullName) || x.FullName.Contains(filter.FullName)) &&
                (string.IsNullOrEmpty(filter.Email) || x.Email.Contains(filter.Email)) &&
                (string.IsNullOrEmpty(filter.PhoneNumber) || x.PhoneNumber.Contains(filter.PhoneNumber)) &&
                (string.IsNullOrEmpty(filter.RequestType) || x.RequestType.Contains(filter.RequestType)) &&
                (filter.Status == null || x.Status == filter.Status.Value) &&
                (filter.PreferredReplyChannel == null || x.PreferredReplyChannel == filter.PreferredReplyChannel) &&
                (filter.RepliedVia == null || x.RepliedVia == filter.RepliedVia);

            var totalCount = await _unitOfWork.ContactRequest.CountAsync(criteria);

            var result = await _unitOfWork.ContactRequest.FindAllAsync(
                criteria: criteria,
                skip: skip,
                take: pagination.PageSize,
                orderBy: x => x.CreatedAt,
                orderByDirection: OrderBy.Descending
            );

            var dtos = result.Select(MapToDto).ToList();

            return Ok(new PagedResponse<ContactRequestDto>
            {
                Data = dtos,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                Succeeded = true,
                Message = dtos.Any() ? ResponseMessages.DataRetrieved : ResponseMessages.NotFound
            });
        }

        [HttpGet("{id}")]
        [isAllowed("إدارة الطلبات", "is_displayed")]
        public async Task<IActionResult> GetById(long id)
        {
            var entity = await _unitOfWork.ContactRequest.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            return Ok(ApiBaseResponse<ContactRequestDto>.Success(MapToDto(entity), ResponseMessages.DataRetrieved));
        }

        [HttpPut("UpdateStatus/{id}")]
        [isAllowed("إدارة الطلبات", "is_update")]
        public async Task<IActionResult> UpdateStatus(long id, ContactRequestStatusUpdateDto model)
        {
            var entity = await _unitOfWork.ContactRequest.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.Status = model.Status;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model.Status == ContactRequestStatus.Closed)
            {
                entity.ClosedAt = DateTime.UtcNow;
            }
            else
            {
                entity.ClosedAt = null;
            }

            _unitOfWork.ContactRequest.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<ContactRequestDto>.Success(MapToDto(entity), ResponseMessages.DataUpdated));
        }

        [HttpPut("Reply/{id}")]
        [isAllowed("إدارة الطلبات", "is_update")]
        public async Task<IActionResult> Reply(long id, ContactRequestReplyDto model)
        {
            var entity = await _unitOfWork.ContactRequest.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.AdminReplyMessage = model.ReplyMessage;
            entity.RepliedVia = model.ReplyChannel;
            entity.RepliedAt = DateTime.UtcNow;
            entity.HandledBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = entity.HandledBy;

            entity.Status = model.CloseRequest ? ContactRequestStatus.Closed : ContactRequestStatus.InProgress;
            entity.ClosedAt = model.CloseRequest ? DateTime.UtcNow : null;

            _unitOfWork.ContactRequest.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            if (model.ReplyChannel == ReplyChannel.Email)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        [entity.Email],
                        "الرد على طلب تواصل معنا",
                        $"<p>مرحبًا {entity.FullName}</p><p>{model.ReplyMessage}</p>");
                }
                catch
                {
                    return Ok(ApiBaseResponse<ContactRequestDto>.Success(
                        MapToDto(entity),
                        "تم حفظ الرد وتحديث الحالة، لكن فشل إرسال البريد الإلكتروني"));
                }
            }

            return Ok(ApiBaseResponse<ContactRequestDto>.Success(MapToDto(entity), ResponseMessages.DataUpdated));
        }

        [HttpPut("SoftDelete/{id}")]
        [isAllowed("إدارة الطلبات", "is_delete")]
        public async Task<IActionResult> SoftDelete(long id)
        {
            var entity = await _unitOfWork.ContactRequest.FindAsync(x => x.Id == id && x.IsDeleted != true);

            if (entity == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _unitOfWork.ContactRequest.Update(entity);
            var result = await _unitOfWork.CompleteAsync();

            if (result == 0)
                return Ok(ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed));

            return Ok(ApiBaseResponse<string>.Success(string.Empty, ResponseMessages.DataDeleted));
        }

        private static ContactRequestDto MapToDto(ContactRequest entity)
        {
            return new ContactRequestDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                RequestType = entity.RequestType,
                Message = entity.Message,
                PreferredReplyChannel = entity.PreferredReplyChannel,
                Status = entity.Status,
                AdminReplyMessage = entity.AdminReplyMessage,
                RepliedVia = entity.RepliedVia,
                CreatedAt = entity.CreatedAt,
                RepliedAt = entity.RepliedAt,
                ClosedAt = entity.ClosedAt,
                HandledBy = entity.HandledBy
            };
        }
    }
}