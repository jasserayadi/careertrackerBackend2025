using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AuthService authService,
            ApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. Local authentication
                var user = _authService.Authenticate(request.Username, request.Password);
                if (user == null)
                {
                    return Unauthorized(new { Message = "Invalid username or password" });
                }

                // 2. Moodle integration
                var moodleResult = await _authService.AuthenticateWithMoodle(request.Username, request.Password);

                // 3. Update Moodle user ID if verification succeeded
                if (moodleResult.Success && moodleResult.MoodleUser != null)
                {
                    user.MoodleUserId = moodleResult.MoodleUser.Id;
                    await _context.SaveChangesAsync();
                }

                // 4. Generate JWT token
                var token = _authService.GenerateJwtToken(user);

                // 5. Prepare response
                var response = new LoginResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Firstname = user.Firstname,
                        Lastname = user.Lastname,
                        MoodleUserId = user.MoodleUserId
                    },
                    Moodle = new MoodleAuthResponseDto
                    {
                        Success = moodleResult.Success,
                        LoginUrl = moodleResult.RedirectUrl,
                        DashboardUrl = moodleResult.DashboardUrl,
                        MoodleUserId = moodleResult.MoodleUser?.Id
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Username}", request.Username);
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
        public MoodleAuthResponseDto Moodle { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int? MoodleUserId { get; set; }
    }

    public class MoodleAuthResponseDto
    {
        public bool Success { get; set; }
        public int? MoodleUserId { get; set; }
        public string LoginUrl { get; set; }
        public string DashboardUrl { get; set; }
    }
}