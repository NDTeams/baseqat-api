using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Services
{
    public interface IEmailService
    {
       
            Task SendEmailAsync(
            List<string> toEmails,
            string subject,
            string body,
            List<string>? ccEmails = null,
            List<string>? bccEmails = null,
            string? fromName = null);
        
    }
}
