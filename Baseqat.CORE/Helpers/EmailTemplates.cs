using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Helpers
{
    public static class EmailTemplates
    {
        public static string GetConfirmationEmail(string userName, string confirmationLink)
        {
            string htmlTemplate = File.ReadAllText("Templates/ConfirmationEmail.html");

            return htmlTemplate
                .Replace("{{UserName}}", userName)
                .Replace("{{ConfirmationLink}}", confirmationLink);
        }

        public static string GetResetPasswordEmail(string userName, string resetLink, string token)
        {
            string htmlTemplate = File.ReadAllText("Templates/ForgetPassword.html");
            return htmlTemplate
                .Replace("{{UserName}}", userName)
                .Replace("{{ResetPasswordLink}}", resetLink)
                .Replace("{{token}}", token);
        }

        public static string GetWelcomeEmail(string userName)
        {
            return $@"
            <h2>مرحباً بك {userName}!</h2>
            <p>شكراً لانضمامك إلى منصتنا. نتمنى لك تجربة رائعة.</p>
        ";
        }

    }
}
