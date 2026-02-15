using Baseqat.CORE.DTOs;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.EF.Consts;
using Baseqat.EF.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Baseqat.CORE.Services
{
    public class AuthServices : IAuthServices

    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;

        public AuthServices(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager
            , IConfiguration configuration,
            IEmailService emailService, ITokenService tokenService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _tokenService = tokenService;
            _signInManager = signInManager;

        }

        public async Task<ApiBaseResponse<string>> Register(RegisterUserDto model)
        {
            // Check if email is provided and already taken
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    if (existingUserByEmail.EmailConfirmed == true)
                    {
                        return ApiBaseResponse<string>.Fail(ResponseMessages.EmailInUse);
                    }
                    else
                    {
                        // User exists but Email not confirmed, remove user
                       

                        var resultDelete = await _userManager.DeleteAsync(existingUserByEmail);
                        if (!resultDelete.Succeeded)
                        {
                            return ApiBaseResponse<string>
                                .Fail(ResponseMessages.OperationFailed,
                                resultDelete.Errors.Select(e => e.Description).ToArray());
                        }
                        // Continue to registration as new user
                    }
                }
            }

            // Check if phone number is provided and already taken
            //if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            //{
            //    var _all = _userManager.Users.ToList();
            //    var existingUserByPhone = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.PhoneNumber);
            //    if (existingUserByPhone != null)
            //    {
            //        if (existingUserByPhone.PhoneNumberConfirmed == true)
            //        {
            //            return ApiBaseResponse<string>.Fail(ResponseMessages.PhoneNumberInUse);
            //        }
            //        else
            //        {
            //            // User exists but phone number not confirmed, remove user
                    

            //            var resultDelete = await _userManager.DeleteAsync(existingUserByPhone);
            //            if (!resultDelete.Succeeded)
            //            {
            //                return ApiBaseResponse<string>
            //                    .Fail(ResponseMessages.OperationFailed,
            //                    resultDelete.Errors.Select(e => e.Description).ToArray());
            //            }
            //            // Continue to registration as new user
            //        }

            //    }
            //}

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName,
                JoinedDate = DateTime.UtcNow,

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return ApiBaseResponse<string>.Fail(ResponseMessages.OperationFailed,
                    result.Errors.Select(e => e.Description).ToArray());
            }


            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    // 1. توليد توكن الـ Identity الأصلي
                    var identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // 2. إنشاء JWT يحتوي على المعرف والتوكن الأصلي (تغليف)
                    var jwtToken =await _tokenService.GenerateEmailPayloadToken(user.Id, identityToken);

                    // 3. بناء الرابط ببارامتر واحد فقط (نظيف وآمن)
                    var confirmationLink = $"{_configuration["AppSettings:ClientUrl"]}/api/account/ConfirmEmail?token={jwtToken}";

                    // 4. إرسال الإيميل
                    var emailBody = EmailTemplates.GetConfirmationEmail(user.UserName, confirmationLink);
                    await _emailService.SendEmailAsync(new List<string> { user.Email }, "تأكيد البريد الإلكتروني", emailBody);
                }
            }
            string ms = ResponseMessages.EmailConfirmationSent;
           
            return ApiBaseResponse<string>.Success(null, ms);
        }

        public async Task<ApiBaseResponse<bool>> ConfirmEmail(string token)
        {
            var (userId, identityToken) = await _tokenService.DecodeEmailPayloadTokenAsync(token);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiBaseResponse<bool>.Fail(ResponseMessages.NotFound);

            // Check if email is already confirmed
            if (user.EmailConfirmed)
                return ApiBaseResponse<bool>.Fail("Email already confirmed.");

            try
            {
                var result = await _userManager.ConfirmEmailAsync(user, identityToken);
                if (!result.Succeeded)
                {
                    // Check if the error is due to token expiration
                    var isExpired = result.Errors.Any(e => e.Code == "InvalidToken");
                    if (isExpired)
                        return ApiBaseResponse<bool>.Fail("Token is invalid or expired (over 30 minutes).");
                    return ApiBaseResponse<bool>.Fail(ResponseMessages.OperationFailed,
                        result.Errors.Select(e => e.Description).ToArray());
                }
                return ApiBaseResponse<bool>.Success(true, ResponseMessages.EmailConfirmed);
            }
            catch (Exception ex)
            {
                // If token is expired or invalid, return false
                return ApiBaseResponse<bool>.Fail("Token is invalid or expired (over 30 minutes).");
            }
        }

        public async Task<ApiBaseResponse<AuthModel>> LoginByEmail(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return ApiBaseResponse<AuthModel>.Fail(ResponseMessages.NotFound);

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return ApiBaseResponse<AuthModel>.Fail(ResponseMessages.InvalidCredentials);

            if (!user.EmailConfirmed)
                return ApiBaseResponse<AuthModel>.Fail(ResponseMessages.EmailNotConfirmed);


            var token = await _tokenService.GenerateTokenAsync(user);

            var authModel = new AuthModel
            {
                IsAuthenticated = true,
                FullName = user.FullName,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                Token = token,
                ExpiresOn = DateTime.UtcNow.AddHours(1),
            };

            return ApiBaseResponse<AuthModel>.Success(authModel, ResponseMessages.DataRetrieved);
        }

        public async Task<ApiBaseResponse<bool>> ForgetPasswordByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ApiBaseResponse<bool>.Fail(ResponseMessages.NotFound);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Build reset password link
            var encodedToken = WebUtility.UrlEncode(resetToken);
            var resetLink = $"{_configuration["AppSettings:ClientUrl"]}/reset?email={WebUtility.UrlEncode(email)}&token={encodedToken}";

            // Prepare email body using a template (similar to Register)
            var emailBody = EmailTemplates.GetResetPasswordEmail(user.UserName, resetLink, resetToken);
            await _emailService.SendEmailAsync(
                new List<string> { user.Email },
                "إعادة تعيين كلمة المرور",
                emailBody
            );

            return ApiBaseResponse<bool>.Success(true, ResponseMessages.PasswordResetSent);
        }

        public async Task<ApiBaseResponse<bool>> VerifyPasswordResetTokenAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ApiBaseResponse<bool>.Fail(ResponseMessages.NotFound);

            // 2. استخدام ميزة Identity للتحقق من التوكن
            // "ResetPassword" هو الغرض (Purpose) من التوكن الذي تم توليده عند طلب نسيان كلمة المرور
            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                token);

            if (!isValid)
                return ApiBaseResponse<bool>.Fail(ResponseMessages.InvalidOrExpiredVerificationLink);

            return ApiBaseResponse<bool>.Success(true, ResponseMessages.PasswordResetSent);
        }

        public async Task<ApiBaseResponse<bool>> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return ApiBaseResponse<bool>.Fail(ResponseMessages.NotFound);

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            
            if (!result.Succeeded)
            {
                return ApiBaseResponse<bool>.Fail(
                    ResponseMessages.OperationFailed,
                    result.Errors.Select(e => e.Description).ToArray());
            }

            return ApiBaseResponse<bool>.Success(true, ResponseMessages.PasswordResetSuccess);
        }
    }
}
