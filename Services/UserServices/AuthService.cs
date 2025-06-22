using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Career_Tracker_Backend.Services.UserServices
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IMoodleService _moodleService;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IMoodleService moodleService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _moodleService = moodleService;
        }

        public string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("JWT Key is missing in configuration.");
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("userId", user.UserId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task MigratePasswordsAsync()
        {
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                if (!user.Password.StartsWith("$2a$") && !user.Password.StartsWith("$2b$"))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
            }
            await _context.SaveChangesAsync();
        }

        public User Authenticate(string username, string password)
        {
            var user = _context.Users
                .Include(u => u.Role) // Ensure Role is loaded
                .FirstOrDefault(u => u.Username == username); // Use the username parameter
            if (user == null)
                return null;

            try
            {
                // Try BCrypt verification first
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    return user;
                }
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Fallback for invalid BCrypt hashes (likely plaintext)
                if (user.Password == password)
                {
                    // Upgrade the password to hashed
                    user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                    _context.SaveChanges();
                    return user;
                }
            }

            return null;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    _logger.LogWarning($"Password reset requested for non-existent email: {email}");
                    return false;
                }

                _logger.LogInformation($"Sending password reset email to {email} for user {user.UserId} with Firstname: {user.Firstname}");

                var token = GenerateResetToken(user.UserId);
                await SendEmailAsync(user, token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending password reset email for {email}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword, string confirmPassword)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int? userId = ValidateResetToken(token);
                if (!userId.HasValue)
                {
                    _logger.LogWarning($"Invalid or expired password reset token: {token}");
                    return false;
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for ID: {userId}");
                    return false;
                }

                if (newPassword != confirmPassword)
                {
                    throw new Exception("Passwords do not match.");
                }

                if (!IsPasswordValid(newPassword))
                {
                    throw new Exception("Password must have at least 8 characters, including 1 digit, 1 lowercase, 1 uppercase, and 1 special character.");
                }

                user.Password = HashPassword(newPassword);

                if (user.MoodleUserId.HasValue)
                {
                    var moodleUpdated = await _moodleService.UpdateMoodleUserAsync(
                        user.MoodleUserId.Value,
                        user.Username,
                        user.Firstname,
                        user.Lastname,
                        user.Email,
                        newPassword);

                    if (!moodleUpdated)
                    {
                        throw new Exception("Failed to update password in Moodle.");
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error resetting password with token: {token}");
                throw;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            int? userId = ValidateResetToken(token);
            if (!userId.HasValue)
                return false;

            var user = await _context.Users.AnyAsync(u => u.UserId == userId.Value);
            return user;
        }

        private string GenerateResetToken(int userId)
        {
            var key = _configuration["Jwt:ResetKey"];
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("JWT ResetKey is missing in configuration.");
                throw new InvalidOperationException("JWT ResetKey is not configured.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int? ValidateResetToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _configuration["Jwt:ResetKey"];
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("JWT ResetKey is missing in configuration.");
                return null;
            }

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                var userIdClaim = principal.FindFirst("userId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return null;
                }

                return userId;
            }
            catch
            {
                return null;
            }
        }

        private async Task SendEmailAsync(User user, string token)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var baseUrl = smtpSettings["ResetPasswordUrl"]; // http://localhost:3000/
            var resetPath = "/Pages/UserPages/reset-password/" + HttpUtility.UrlEncode(token); // Dynamic path with token
            var resetUrl = baseUrl + resetPath;

            _logger.LogInformation($"Generated reset URL: {resetUrl}");

            var message = new MailMessage
            {
                From = new MailAddress(smtpSettings["SenderEmail"], smtpSettings["SenderName"]),
                Subject = "Password Reset Request",
                IsBodyHtml = true,
                Body = $@"<h2>Password Reset Request</h2>
                        <p>Hello {HttpUtility.HtmlEncode(user.Firstname)},</p>
                        <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                        <p><a href='{resetUrl}'>Reset Password</a></p>
                        <p>This link will expire in 1 hour.</p>
                        <p>If you did not request a password reset, please ignore this email.</p>"
            };
            message.To.Add(user.Email);

            using (var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]))
            {
                Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"])
            })
            {
                await client.SendMailAsync(message);
            }
        }

        private bool IsPasswordValid(string password)
        {
            var regex = new Regex(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$");
            return regex.IsMatch(password);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}