using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Baseqat.CORE.Helpers;
using Baseqat.CORE.Response;
using Baseqat.EF.DATA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Baseqt.API.Helper
{
    public class isAllowedFilter : IAsyncAuthorizationFilter
    {
        //public readonly string[] _roles;
        public readonly string _privlige;
        public readonly string _permession;
        private readonly AppDbContext db;
        private readonly UsersHelper usersHelper;
        public isAllowedFilter(AppDbContext context, UsersHelper usersHelper, string privlige, string permession/*, params string[] roles*/)
        {
            //this._roles = roles;
            _privlige = privlige;
            _permession = permession;
            db = context;
            this.usersHelper = usersHelper;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            // Get the CustomRole attribute
            //var attribute = context.ActionDescriptor.EndpointMetadata
            //    .OfType<isAllowed>().FirstOrDefault();
            var dresult = false;
            //if (attribute == null)
            //     return;

            // Example: Get current user from claims
            //var userName = context.HttpContext.User.Identity?.Name;
            //if (userName == null)
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {

                context.Result = new JsonResult(ApiBaseResponse<string>.Fail("Access Denied.."))
                {
                    StatusCode = 401
                };
                return;
            }
            // Try to find user by name first, then fallback to ID from JWT claims
            Baseqat.EF.Models.Auth.ApplicationUser? usr = null;
            var userName = user.Identity?.Name;
            if (!string.IsNullOrEmpty(userName))
                usr = await usersHelper.getUserInfobyName(userName);

            if (usr == null)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? user.FindFirstValue("sub")
                    ?? user.FindFirstValue("unique_name");
                if (!string.IsNullOrEmpty(userId))
                    usr = await usersHelper.getUserInfobyId(userId);
            }

            if (usr == null)
            {
                context.Result = new JsonResult(ApiBaseResponse<string>.Fail("access Denied.."))
                {
                    StatusCode = 401
                };
                return;
            }
            if (/*this._roles.Any()&& */!string.IsNullOrEmpty(this._permession) && !string.IsNullOrEmpty(this._privlige))
            {


                var roles = await usersHelper.ListUserRoles(usr.Id);

                if (roles == null || !roles.Any())
                {
                    context.Result = new JsonResult(ApiBaseResponse<string>.Fail("access Denied.."))
                    {
                        StatusCode = 401 // HTTP status code for Internal Server Error
                    }; //new UnauthorizedResult();
                    return;
                }

                // SuperAdmin has all privileges
                if (roles.Contains("SuperAdmin"))
                {
                    return; // Allow access
                }

                //var roles = this._roles;
                var prv = db.Privileges.FirstOrDefault(a => a.priv_name == this._privlige);
                if (prv != null)
                {
                    foreach (var role in roles)
                    {
                        var r = await usersHelper.getRoleInfoByName(role);
                        if (r != null)
                        {
                            //var is_in_role = user.IsInRole(role);//check if user in this role
                            var isex = db.Privileges_RoleBased.FirstOrDefault(a => a.PrivilegesId == prv.Id && a.RoleId == r.Id);
                            if (isex != null)
                            {
                                if (this._permession == "*")
                                    dresult = (isex.is_displayed || isex.is_insert || isex.is_delete || isex.is_update || isex.is_print ? true : false);
                                else if (this._permession == "is_displayed")
                                    dresult = isex.is_displayed;
                                else if (this._permession == "is_insert")
                                    dresult = isex.is_insert;
                                else if (this._permession == "is_update")
                                    dresult = isex.is_update;
                                else if (this._permession == "is_delete")
                                    dresult = isex.is_delete;
                                else if (this._permession == "is_print")
                                    dresult = isex.is_print;

                            }
                        }
                    }


                }

            }
            ///
            if (dresult == false)
            {

                context.Result = new JsonResult(ApiBaseResponse<string>.Fail("access Denied.."))
                {
                    StatusCode = 401 // HTTP status code for Internal Server Error
                }; //new UnauthorizedResult();
                return;
            }

        }
    }
}
