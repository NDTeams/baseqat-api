using Baseqat.CORE.Helpers;
using Baseqat.EF.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Baseqat.CORE.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly byte[] _key;
        private readonly IRoleService _roleService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TokenService(IOptions<JwtSettings> jwtOptions, IRoleService roleService,
            RoleManager<IdentityRole> roleManager)
        {
            _jwtSettings = jwtOptions.Value;
            _key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            _roleService = roleService;
            _roleManager = roleManager;
        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var roles = await _roleService.GetRolesForUserAsync(user.Id);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            foreach (var roleName in roles.Data)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                var role = await _roleManager.FindByNameAsync(roleName);
                var roleClaims = await _roleManager.GetClaimsAsync(role!);
                claims.AddRange(roleClaims);
            }
            var credentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public Task<string> ValidateTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
                return Task.FromResult(userId);
            }
            catch
            {
                return Task.FromResult<string>(null);
            }
        }

        public Task<string> RefreshTokenAsync(string token, string refreshToken)
        {
            return ValidateTokenAsync(token).ContinueWith(async t =>
            {
                var userId = t.Result;
                if (userId == null)
                    return null;

                var user = new ApplicationUser { Id = userId, UserName = "RefreshedUser" };
                return await GenerateTokenAsync(user);
            }).Unwrap();
        }

        public Task<string> GetUserIdByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Task.FromResult<string>(null);

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
                return Task.FromResult(userId);
            }
            catch
            {
                return Task.FromResult<string>(null);
            }
        }

        public Task<string> GenerateEmailPayloadToken(string userId, string identityToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // استخدمنا مسميات واضحة وموحدة
            var claims = new List<Claim>
    {
        new Claim("uid", userId),
        new Claim("itoken", identityToken),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // تأكد أن _key هنا هو نفس المفتاح المستخدم في الفك
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        public async Task<(string? userId, string? identityToken)> DecodeEmailPayloadTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return (null, null);

            var tokenHandler = new JwtSecurityTokenHandler();
            // تأكد أن المفتاح هنا يطابق تماماً المفتاح في التوليد
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                
                var userId = principal.FindFirst("uid")?.Value;
                var identityToken = principal.FindFirst("itoken")?.Value;

                return (userId, identityToken);
            }
            catch (Exception ex)
            {
                // إذا استمرت المشكلة، اطبع ex.Message هنا لتعرف السبب بدقة
                return (null, null);
            }
        }
    }
}
