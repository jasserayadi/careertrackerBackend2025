using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;

namespace Career_Tracker_Backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMoodleService _moodleService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, IMoodleService moodleService, ApplicationDbContext context, ILogger<UserController> logger)
        {
            _userService = userService;
            _moodleService = moodleService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserRequest request)
        {
            if (request.CvFile == null || request.CvFile.Length == 0)
            {
                return BadRequest("CV file is required.");
            }

            var result = await _userService.RegisterUser(
                request.Username,
                request.Firstname,
                request.Lastname,
                request.Password,
                request.confirmPassword,
                request.Email,
                request.CvFile
            );

            if (!result)
            {
                return BadRequest("User registration failed.");
            }

            return Ok("User registered successfully.");
        }

        public class RegisterUserRequest
        {
            public string Username { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public IFormFile CvFile { get; set; }
            public RoleName Role { get; set; }
            public string confirmPassword { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.CV)
                    .Include(u => u.Job)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Firstname,
                        u.Lastname,
                        u.Email,
                        u.DateCreation,
                        Role = u.Role != null ? new { RoleName = u.Role.RoleName.ToString() } : null,
                        CV = u.CV != null ? new { u.CV.CvFile } : null,
                        Job = u.Job != null ? new { u.Job.JobId, u.Job.JobName } : null
                    })
                    .ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "Utilisateur non trouvé." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);

                if (result)
                {
                    return Ok(new { message = "User deleted successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to delete user." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("id/{id:int}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("completion/{userId}/{courseId}")]
        public async Task<IActionResult> GetUserCourseCompletionStatus(int userId, int courseId)
        {
            try
            {
                var completionStatus = await _userService.GetUserCourseCompletionStatusAsync(userId, courseId);
                return Ok(completionStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("grades/{courseId}/{userId}")]
        public async Task<ActionResult<List<MoodleGradeItem>>> GetUserGrades(int courseId, int userId)
        {
            try
            {
                var grades = await _moodleService.GetUserGradesAsync(courseId, userId);

                if (grades == null || grades.Count == 0)
                {
                    return NotFound("No grades found for this user in the specified course");
                }

                return Ok(grades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{userId}/assign-job")]
        public async Task<IActionResult> AssignJobToUser(int userId, [FromBody] AssignJobRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Job)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) return NotFound("User not found");

                var job = await _context.Jobs.FindAsync(request.JobId);
                if (job == null) return NotFound("Job not found");

                user.JobId = request.JobId;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Job assigned successfully",
                    JobName = job.JobName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error assigning job: {ex.Message}");
            }
        }

        public class AssignJobRequest
        {
            public int JobId { get; set; }
        }

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser(
            int userId,
            [FromForm] UpdateUserRequest request)
        {
            try
            {
                _logger.LogInformation($"Updating user {userId}");
                _logger.LogInformation($"Username: {request.Username}");
                _logger.LogInformation($"Email: {request.Email}");
                _logger.LogInformation($"CV File: {(request.CvFile != null ? request.CvFile.FileName : "null")}");

                var result = await _userService.UpdateUserAsync(
                    userId,
                    request.Username,
                    request.Firstname,
                    request.Lastname,
                    request.Email,
                    request.Password,
                    request.ConfirmPassword,
                    request.CvFile);

                if (!result)
                {
                    return BadRequest(new { message = "User update failed." });
                }

                var updatedUser = await _context.Users
                    .Include(u => u.CV)
                    .Include(u => u.Role)
                    .Include(u => u.Job)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {userId}");
                return BadRequest(new { message = ex.Message });
            }
        }

        public class UpdateUserRequest
        {
            [Required]
            public string? Username { get; set; }

            [Required]
            public string? Firstname { get; set; }

            [Required]
            public string? Lastname { get; set; }

            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            public string? Password { get; set; }

            [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
            public string? ConfirmPassword { get; set; }

            public IFormFile? CvFile { get; set; }
        }
    }
}