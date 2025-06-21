using Microsoft.Extensions.Logging;
using Resend;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IResend resend, ILogger<EmailService> logger)
        {
            _resend = resend ?? throw new ArgumentNullException(nameof(resend));
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = "onboarding@resend.dev"; // Replace with verified custom domain email if available
            if (string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogError("Resend FromEmail is not configured.");
                throw new Exception("Email configuration is missing.");
            }

            var message = new EmailMessage
            {
                From = fromEmail,
                To = { toEmail }, // Use collection for multiple recipients if needed
                Subject = subject,
                HtmlBody = $"<h2>Welcome to Career Tracker!</h2><p>{body}</p>"
            };

            try
            {
                var response = await _resend.EmailSendAsync(message);
                _logger.LogInformation("Email sent successfully to {ToEmail} with ID {EmailId}", toEmail, response.Content);
            }
            catch (ResendException ex)
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