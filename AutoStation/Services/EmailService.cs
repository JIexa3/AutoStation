using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AutoStation.Services
{
    public class EmailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "autostationnn234@gmail.com"; 
        private const string SenderPassword = "qbax wmgq hahh dvth"; 

        public async Task SendVerificationCodeAsync(string toEmail, string verificationCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("AutoStation", SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Подтверждение регистрации в AutoStation";
                message.Body = new TextPart("plain")
                {
                    Text = $"Ваш код подтверждения: {verificationCode}\n\nВведите этот код в приложении для завершения регистрации."
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(SenderEmail, SenderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при отправке email: {ex.Message}", ex);
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AutoStation", SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(SenderEmail, SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }
    }
}
