using Baseqat.CORE.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Baseqt.API.Middleware
{
    public class JwtBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            // استخراج التوكن من الهيدر
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // التحقق من أن التوكن ليس في القائمة السوداء
                var isRevoked = await tokenService.IsTokenRevokedAsync(token);
                
                if (isRevoked)
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"succeeded\":false,\"message\":\"تم إلغاء التوكن. يرجى تسجيل الدخول مرة أخرى.\"}");
                    return;
                }
            }

            await _next(context);
        }
    }
}
