using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("SecureFrontend")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        // Static instance of CustomCorsFilter since DI registration isn't available
        private static readonly CustomCorsFilter _corsFilter = new CustomCorsFilter();

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration,
            IAuthService authService)
        {
            _context = context;
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _authService.SendPasswordResetEmailAsync(request.Email);
                // Always return OK to prevent email enumeration
                return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(
                    request.Token,
                    request.NewPassword,
                    request.ConfirmPassword);

                if (!result)
                {
                    return BadRequest(new { message = "Invalid or expired reset token." });
                }

                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] ValidateTokenRequest request)
        {
            // Create action arguments dictionary (empty for this case)
            var actionArguments = new Dictionary<string, object?>
            {
                { "request", request }
            };

            // Apply CORS manually
            _corsFilter.OnActionExecuting(new ActionExecutingContext(
                new ActionContext(HttpContext, new RouteData(), new ControllerActionDescriptor()),
                new List<IFilterMetadata>(),
                actionArguments,
                this
            ));

            try
            {
                var isValid = await _authService.ValidateResetTokenAsync(request.Token);
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while validating the token." });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _authService.Authenticate(request.Username, request.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var token = _authService.GenerateJwtToken(user);

            Response.Cookies.Append(
                "userId",
                user.UserId.ToString(),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

            Response.Cookies.Append(
                "token",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

            return Ok(new
            {
                Message = "Login successful",
                Username = user.Username,
                UserId = user.UserId,
                role = user.Role?.RoleName.ToString() ?? "Unknown"
            });
        }

        [HttpGet("session")]
        public IActionResult GetSession()
        {
            if (!Request.Cookies.TryGetValue("userId", out var userId) ||
                !Request.Cookies.TryGetValue("token", out var token))
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true
                }, out _);

                var user = _context.Users.FirstOrDefault(u => u.UserId.ToString() == userId);
                if (user == null) return Unauthorized();

                return Ok(new
                {
                    username = user.Username,
                    userId = user.UserId,
                    firstname = user.Firstname,
                    lastname = user.Lastname,
                    email = user.Email
                });
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("userId");
            Response.Cookies.Delete("token");
            return Ok(new { message = "Logged out successfully" });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class ValidateTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }

    // Custom CORS filter as a standalone class
    public class CustomCorsFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
            if (context.HttpContext.Request.Method == "OPTIONS")
            {
                context.Result = new StatusCodeResult(204); // Handle preflight
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}