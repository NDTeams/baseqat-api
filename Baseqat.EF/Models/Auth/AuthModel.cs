using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Models.Auth
{
    public class AuthModel
    {
        
        public bool IsAuthenticated { get; set; }

        public string FullName { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public List<string> Roles { get; set; }

        public string Token { get; set; }

        public DateTime ExpiresOn { get; set; }

    }
}
