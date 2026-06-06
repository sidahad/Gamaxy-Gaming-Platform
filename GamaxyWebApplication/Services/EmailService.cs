namespace GamaxyWebApplication.Services
{
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmail(string toEmail, string verificationLink)
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var appPassword = _configuration["EmailSettings:AppPassword"];

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                smtp.EnableSsl = true;

                var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = "Email Verification",
                    Body = $"Please verify your email by clicking this link: {verificationLink}",
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);
            }
        }
    }

}
