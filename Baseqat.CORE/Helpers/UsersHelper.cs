using Baseqat.EF.DATA;
using Baseqat.EF.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Baseqat.CORE.Helpers
{
    public class UsersHelper
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly AppDbContext db;
        public UsersHelper(RoleManager<IdentityRole> _roleManager, 
            UserManager<ApplicationUser> _UserManager, AppDbContext _db)
        {
            this._roleManager = _roleManager;
            this._UserManager = _UserManager;
            this.db = _db;
        }

        public List<IdentityRole> ListRoles()
        {
            return _roleManager.Roles.ToList();
        }
        public List<ApplicationUser> ListUsers()
        {
            return _UserManager.Users.ToList();
        }
        public async Task<IdentityRole> getRoleInfoById(string id)
        {
            //string name = "";
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
                return role;
            return null;
        }
        public async Task<string> getRoleName(string id)
        {
            string name = "";
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
                name = role.Name;
            return name;
        }
        public async Task<ApplicationUser> getUserInfobyphone(string phone)
        {
            if (!string.IsNullOrEmpty(phone))
            {
                var u = await _UserManager.Users.Where(c => c.PhoneNumber == phone).FirstOrDefaultAsync();

                if (u != null)
                    return u;
            }
            return null;
        }

        public async Task<ApplicationUser> getUserInfobyEmail(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var u = await _UserManager.FindByEmailAsync(email);
                if (u != null)
                    return u;
            }
            return null;
        }

        public async Task<ApplicationUser> getUserInfobyId(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var u = await _UserManager.FindByIdAsync(id);
                if (u != null)
                    return u;
            }
            return null;
        }

        public async Task<ApplicationUser> getUserInfobyName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var u = await _UserManager.FindByNameAsync(name);
                if (u != null)
                    return u;
            }
            return null;
        }

        public async Task<bool> RemoveUserFromAllRoles(string userId)
        {
            var user = await _UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await _UserManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    var result = await _UserManager.RemoveFromRolesAsync(user, roles);
                    return result.Succeeded;
                }
            }
            return false;
        }

        public async Task<bool> AddUserToRoleByname(string userId, string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                bool b = await AddUserToRole(userId, role.Id);
                return b;
            }
            return false;
        }

        public async Task<bool> AddUserToRole(string userId, string roleName)
        {
            bool isRemoved = await RemoveUserFromAllRoles(userId);
            // if (isRemoved)
            //{
            var user = await _UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var result = await _UserManager.AddToRoleAsync(user, role.Name);
                    return true;
                }
            }

            //}
            return false;
        }

        public async Task<List<ApplicationUser>> ListUsersInRole(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                // Find the role by name
                var role = await _roleManager.FindByIdAsync(id);

                if (role == null)
                {
                    // Role does not exist
                    return null;
                }
                // Get the list of users in the role
                var usersInRole = await _UserManager.GetUsersInRoleAsync(role.Name);
                return usersInRole.ToList();
            }
            return null;

        }

        public async Task<bool> RemoveUser(string userId)
        {
            // Find the user by ID
            var user = await _UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                // User does not exist
                return false;
            }

            // Delete the user
            var result = await _UserManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // User deleted successfully
                return true;
            }
            return false;
        }

        public async Task<IdentityRole> getRoleInfoByName(string name)
        {
            var role = await _roleManager.FindByNameAsync(name);
            if (role != null)
                return role;
            return null;

        }

        public async Task<bool> SetUserPassword(string userId, string newPassword)
        {
            // Find the user by ID
            var user = await _UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                // User does not exist
                return false;
            }
            var result1 = await _UserManager.RemovePasswordAsync(user);

            if (result1.Succeeded)
            {
                var result = await _UserManager.AddPasswordAsync(user, newPassword);
                if (result.Succeeded)
                {
                    // User deleted successfully
                    return true;
                }
            }
            return false;
        }

        public async Task<List<string>> ListUserRoles(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            var user = await _UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                // User does not exist
                return null;
            }
            var roles = await _UserManager.GetRolesAsync(user);
            if (roles.Any())
            {
                return roles.ToList();
            }
            return null;
        }

        public async Task<string> getUserRole(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return string.Empty;
            var user = await _UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                // User does not exist
                return string.Empty;
            }
            var roles = await _UserManager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                return roles[0];
            }
            return string.Empty;
        }

        public async Task<string> getUserRoleId(string userId)
        {

            string roleName = await getUserRole(userId);
            if (!string.IsNullOrEmpty(roleName))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                    return role.Id;
            }

            return string.Empty;
        }

        public async Task<ApplicationUser> getUserByName(string name)
        {
            // Find the user by ID
            if (!string.IsNullOrEmpty(name))
                return await _UserManager.FindByNameAsync(name);
            return null;
        }

        public async Task<bool> createUser(string email, string phone, string password, string role)
        {
            ApplicationUser us = new ApplicationUser();
            us.Email = email;
            us.UserName = email;

            us.PhoneNumber = phone;
            us.Id = Guid.NewGuid().ToString();

            var result = await _UserManager.CreateAsync(us, password);

            if (result.Succeeded)
            {
                //get user info
                var nw_user = await _UserManager.FindByIdAsync(us.Id);
                //add th user to role
                //1.Remove from all role
                if (nw_user != null)
                {
                    bool a = await AddUserToRoleByName(nw_user.Id, role);
                }
                //await db.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> AddUserToRoleByName(string userId, string roleName)
        {
            bool isRemoved = await RemoveUserFromAllRoles(userId);
            // if (isRemoved)
            //{
            var user = await _UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var result = await _UserManager.AddToRoleAsync(user, role.Name);
                    return true;
                }
            }

            //}
            return false;
        }

        public async Task<bool> setUserLocked(string userId)
        {
            // Find the user by their ID
            var user = await getUserInfobyId(userId);

            if (user != null)
            {
                await _UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                return true;
            }

            // Lock the user
            return false;

        }

        public async Task<bool> setUserUnLocked(string userId)
        {
            // Find the user by their ID
            var user = await getUserInfobyId(userId);

            if (user != null)
            {
                await _UserManager.ResetAccessFailedCountAsync(user);
                await _UserManager.SetLockoutEndDateAsync(user, null);
                return true;
            }

            // Lock the user
            return false;

        }

        public async Task<bool> IsUserLocked(string userId)
        {
            // Find the user by their ID
            var user = await getUserInfobyId(userId);

            if (user != null)
            {
                var isLockedOut = await _UserManager.IsLockedOutAsync(user);
                if (isLockedOut)
                    return true;
                else
                    return false;
            }

            // Lock the user
            return false;

        }

        public async Task<bool> isEmailConfirmed(string userId)
        {
            var user = await getUserInfobyEmail(userId);
            if (user != null)
            {
                string user_role = await getUserRole(user.Id);
                if (!string.IsNullOrEmpty(user_role) && user_role == "المدراء")
                    return true;
                else
                {
                    if (user.EmailConfirmed)
                        return true;
                }
            }

            return false;
        }

        public async Task<bool> isTelephoneConfirmed(string userId)
        {
            var user = await getUserInfobyEmail(userId);
            if (user != null)
            {
                string user_role = await getUserRole(user.Id);
                if (!string.IsNullOrEmpty(user_role) && user_role == "المدراء")
                    return true;
                else
                {
                    if (user.PhoneNumberConfirmed)
                        return true;
                }
            }

            return false;
        }

        public async Task<bool> setUserEmailConfirm(string userId)
        {
            // Find the user by their ID
            var user = await getUserInfobyEmail(userId);

            if (user != null)
            {
                user.EmailConfirmed = true;
                var result = await _UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }
            }

            // Lock the user
            return false;

        }

        public async Task<bool> UpdateUserPhone(string userId, string phone)
        {
            // Find the user by their ID
            var user = await getUserInfobyEmail(userId);

            if (user != null)
            {

                user.PhoneNumber = phone;
                var result = await _UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }
            }

            // Lock the user
            return false;

        }

        public async Task<bool> setUserPhoneConfirm(string userId)
        {
            // Find the user by their ID
            var user = await getUserInfobyEmail(userId);

            if (user != null)
            {
                user.PhoneNumberConfirmed = true;
                var result = await _UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }
            }

            // Lock the user
            return false;

        }

        public async Task<List<Privileges>> ListPriviligesByCateogry(string catname)
        {
            var lst = await db.Privileges.Where(c => c.priv_cat == catname).ToListAsync();
            return lst;
        }

        public async Task<bool> addPriviligesToUser(string userId, string prv_name, bool is_insert, bool is_update, bool is_delete, bool is_displayed, bool isPrint)
        {
            // Find the user by their ID
            var user = await getUserInfobyEmail(userId);
            var prv = db.Privileges.FirstOrDefault(a => a.priv_name == prv_name);
            if (user != null && prv != null)
            {
                var isex = db.Privileges_UserBased.FirstOrDefault(c => c.UserId == user.Id && c.PrivilegesId == prv.Id);
                if (isex == null)
                {
                    Privileges_UserBased pu = new Privileges_UserBased();
                    pu.UserId = user.Id;
                    pu.PrivilegesId = prv.Id;
                    pu.is_delete = is_delete;
                    pu.is_displayed = is_displayed;
                    pu.is_insert = is_insert;
                    pu.is_print = isPrint;
                    pu.is_update = is_update;
                    db.Privileges_UserBased.Add(pu);
                    await db.SaveChangesAsync();
                }
                else
                {
                    isex.is_delete = is_delete;
                    isex.is_displayed = is_displayed;
                    isex.is_insert = is_insert;
                    isex.is_print = isPrint;
                    isex.is_update = is_update;
                    await db.SaveChangesAsync();
                }
                return true;
            }

            // Lock the user
            return false;

        }

    


        public async Task<List<ApplicationUser>> getUsersByRoleName(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var users = await _UserManager.GetUsersInRoleAsync(roleName);
                    return users.ToList();
                }
            }
            return null;
        }

        public async Task<bool> UpdateUserSubribeInPlan(ApplicationUser user)
        {


            if (user != null)
            {


                var result = await _UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }
            }

            // Lock the user
            return false;

        }
    }
}
