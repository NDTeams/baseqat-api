using Baseqat.EF.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Consts
{
    public static class IntitalUsers
    {
        public static readonly ApplicationUser SuperUser =
            new ApplicationUser
            {
                FullName = "مدير النظام",
                UserName = "al-bayti@hotmail.com",
                Email = "al-bayti@hotmail.com",
                PhoneNumber = "966543740920",
                EmailConfirmed = true,
                JoinedDate = DateTime.UtcNow,
                PhoneNumberConfirmed = true

            };
    }
}
