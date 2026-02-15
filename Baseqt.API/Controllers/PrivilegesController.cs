using Baseqat.CORE.Response;
using Baseqat.EF.Consts;
using Baseqat.EF.DATA;
using Baseqat.EF.Models.Auth;
using Baseqt.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrivilegesController : ControllerBase
    {
        private readonly IDataUnit _unitOfWork;

        public PrivilegesController(IDataUnit unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            
        }

        [HttpGet("GetAllPrivileges")]
        [isAllowed("ادارة المجموعات", "is_displayed")]
        public async Task<IActionResult> GetAll()
        {
            var privileges = await _unitOfWork.Privileges.GetAllAsync();
            var dto = privileges.Select(a => new { a.Id, a.priv_name, a.priv_cat, a.priv_key, a.isEnabled });
            if (privileges == null || !privileges.Any())
            {
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));
            }
            return Ok(ApiBaseResponse<object>.Success(dto, ResponseMessages.DataRetrieved));
        }

        [HttpGet("GetPrivilegeById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var privilege = await _unitOfWork.Privileges.GetByIdAsync(id);
            if (privilege == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));
            var dto = new
            {
                privilege.Id,
                privilege.priv_name,
                privilege.priv_cat,
                privilege.priv_key,
                privilege.isEnabled
            };
            return Ok(ApiBaseResponse<object>.Success(dto, ResponseMessages.DataRetrieved));
        }

        [HttpPost("AddPrivilege")]
        public async Task<IActionResult> Add(string priv_name, string priv_cat, bool? isEnabled = true)
        {
            var newPrivilege = new Privileges
            {
                priv_name = priv_name,
                priv_cat = priv_cat,
                isEnabled = isEnabled,
                priv_key = Guid.NewGuid()
            };
            await _unitOfWork.Privileges.AddAsync(newPrivilege);
            await _unitOfWork.CompleteAsync();
            return Ok(ApiBaseResponse<object>.Success(newPrivilege, ResponseMessages.DataSaved));
        }

        [HttpPut("UpdatePrivilege/{id}")]
        public async Task<IActionResult> Update(int id, string priv_name, string priv_cat, bool? isEnabled = true)
        {
            var existingPrivilege = await _unitOfWork.Privileges.GetByIdAsync(id);
            if (existingPrivilege == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));
            existingPrivilege.priv_name = priv_name;
            existingPrivilege.priv_cat = priv_cat;
            existingPrivilege.isEnabled = isEnabled;
            _unitOfWork.Privileges.Update(existingPrivilege);
            await _unitOfWork.CompleteAsync();
            return Ok(ApiBaseResponse<object>.Success(existingPrivilege, ResponseMessages.DataUpdated));
        }


        [HttpDelete("DeletePrivilege/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var privilege = await _unitOfWork.Privileges.GetByIdAsync(id);
            if (privilege == null)
                return NotFound(ApiBaseResponse<string>.Fail(ResponseMessages.NotFound));
            _unitOfWork.Privileges.Delete(privilege);
            await _unitOfWork.CompleteAsync();
            return Ok(ApiBaseResponse<string>.Success("Privilege deleted successfully.", ResponseMessages.DataDeleted));
        }
    }
}
