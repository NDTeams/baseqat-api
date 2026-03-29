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

        public static string GetConsultationCompletedEmail(
            string clientName, string subject, string consultantName,
            DateTime? consultationDate, string? consultationTime)
        {
            var dateStr = consultationDate?.ToString("yyyy/MM/dd") ?? "غير محدد";
            var timeStr = consultationTime ?? "غير محدد";

            return $@"
            <div dir='rtl' style='font-family: Cairo, Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #0d9488, #14b8a6); padding: 30px; border-radius: 16px 16px 0 0; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 24px;'>تم إكمال الاستشارة</h1>
                </div>
                <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 16px 16px;'>
                    <p style='font-size: 16px; color: #374151;'>مرحباً <strong>{clientName}</strong>،</p>
                    <p style='font-size: 14px; color: #6b7280;'>يسعدنا إبلاغك بأن استشارتك قد تم إكمالها بنجاح.</p>
                    <div style='background: #f0fdfa; border: 1px solid #ccfbf1; border-radius: 12px; padding: 20px; margin: 20px 0;'>
                        <table style='width: 100%; font-size: 14px; color: #374151;'>
                            <tr><td style='padding: 8px 0; color: #6b7280;'>الموضوع:</td><td style='padding: 8px 0; font-weight: bold;'>{subject}</td></tr>
                            <tr><td style='padding: 8px 0; color: #6b7280;'>المستشار:</td><td style='padding: 8px 0; font-weight: bold;'>{consultantName}</td></tr>
                            <tr><td style='padding: 8px 0; color: #6b7280;'>التاريخ:</td><td style='padding: 8px 0;'>{dateStr}</td></tr>
                            <tr><td style='padding: 8px 0; color: #6b7280;'>الوقت:</td><td style='padding: 8px 0;'>{timeStr}</td></tr>
                        </table>
                    </div>
                    <p style='font-size: 14px; color: #6b7280;'>نتمنى أن تكون الاستشارة قد أفادتك. لا تتردد في طلب استشارة جديدة في أي وقت.</p>
                    <p style='font-size: 14px; color: #6b7280; margin-top: 20px;'>مع أطيب التحيات،<br/><strong>فريق باسقات</strong></p>
                </div>
            </div>";
        }

    }
}
