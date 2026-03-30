using Baseqat.CORE.DTOs;
using Baseqat.CORE.Response;
using Baseqat.EF.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Services
{
    public interface IAuthServices
    {
        Task<ApiBaseResponse<string>> Register(RegisterUserDto model);

        Task<ApiBaseResponse<bool>> ConfirmEmail(string token);

        Task<ApiBaseResponse<AuthModel>> LoginByEmail(LoginDto model);

        Task<ApiBaseResponse<bool>> ForgetPasswordByEmail(string email);

        Task<ApiBaseResponse<bool>> VerifyPasswordResetTokenAsync(string email, string token);

        Task<ApiBaseResponse<bool>> ResetPassword(ResetPasswordDto model);

        Task<ApiBaseResponse<bool>> Logout(string token);

    }
}
