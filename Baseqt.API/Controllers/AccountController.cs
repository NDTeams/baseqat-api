using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response;
using Baseqat.CORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Baseqt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        public AccountController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiBaseResponse<string>>>
           Register([FromBody] RegisterUserDto model)
        {
            var result = await _authServices.Register(model);
            if (!result.Succeeded)
                return Ok(result);

            return Ok(result);
        }

        [HttpGet("confirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail( string token)
        {
            var result = await _authServices.ConfirmEmail(token);
            if (!result.Succeeded)
            {
              
                return Redirect("/ConfirmMailFail.html");
            }
            // Redirect with success message in query string
            return Redirect("/ConfirmEmailSuccess.html");
        }

        [HttpPost("LoginByEmail")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiBaseResponse<string>>>
            LoginByEmail(LoginDto model)
        {
            var result = await _authServices.LoginByEmail(model);
            if (!result.Succeeded)
                return Ok(result);

            return Ok(result);
        }

        [HttpPost("ForgetPasswordByEmail")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiBaseResponse<bool>>> ForgetPassword(string email)
        {
            var result = await _authServices.ForgetPasswordByEmail(email);
            if (!result.Succeeded)
                return Ok(result);
            return Ok(result);
        }

        [HttpGet("/reset")]
        public async Task<IActionResult> VerifyResetToken(string email, string token)
        {
            // 1. استدعاء الخدمة للتأكد من صحة التوكن والبريد
            // نفترض أن لديك دالة في الـ Service تسمى IsResetTokenValid
            var isValid = await _authServices.VerifyPasswordResetTokenAsync(email, token);

            if (!isValid.Succeeded)
            {
                // إذا كان التوكن خطأ أو منتهي الصلاحية، نوجهه لصفحة الخطأ
                return Redirect("/ResetPasswordError.html");
            }

            // إذا كان صحيحاً، نرسله لصفحة تعيين كلمة المرور الجديدة 
            // ونمرر التوكن والإيميل لضمان استخدامهما عند الحفظ النهائي
            return Redirect($"/RestPassword.html?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");
           
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiBaseResponse<bool>>> ResetPassword(ResetPasswordDto model)
        {
            var result = await _authServices.ResetPassword(model);
            if (!result.Succeeded)
                return Ok(result);
            return Ok(result);
        }

    }
}
