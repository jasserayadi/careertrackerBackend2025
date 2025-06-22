using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Career_Tracker_Backend.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var fromEmail = smtpSettings["SenderEmail"];
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"]);
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"]);

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(host))
            {
                _logger.LogError("SMTP configuration is incomplete.");
                throw new Exception("Email configuration is missing.");
            }

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, "Career Tracker"),
                Subject = subject,
                IsBodyHtml = true,
                Body = $"<h2>Welcome to Career Tracker!</h2><p>{body}</p>"
            };
            message.To.Add(toEmail);

            using (var client = new SmtpClient(host, port)
            {
                Credentials = new System.Net.NetworkCredential(username, password),
                EnableSsl = enableSsl
            })
            {
                try
                {
                    await client.SendMailAsync(message);
                    _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                }
                catch (SmtpException ex)
                {
                    _logger.LogError(ex, "Failed to send email to {ToEmail}: {Message}", toEmail, ex.Message);
                    throw new Exception($"Email sending failed: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                    throw;
                }
            }
        }
    }
}