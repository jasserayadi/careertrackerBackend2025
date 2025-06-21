using Career_Tracker_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BadgeController : ControllerBase
    {
        private readonly IBadgeService _badgeService;
        private readonly ApplicationDbContext _context;

        public BadgeController(IBadgeService badgeService, ApplicationDbContext context)
        {
            _badgeService = badgeService;
            _context = context;
        }

        [HttpPost("{userId}/assign-badge")]
        public async Task<IActionResult> AssignBadge(int userId)
        {
            try
            {
                await _badgeService.AssignBadgeBasedOnCertificatesAsync(userId);
                return Ok(new { Message = "Badge assigned successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

[HttpPost("assign-all-badges")]
        public async Task<IActionResult> AssignBadgesToAllUsers()
        {
            try
            {
                await _badgeService.AssignBadgesToAllUsersAsync();
                return Ok(new { Message = "Badges assigned to all users successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while assigning badges", Error = ex.Message });
            }
        }
        [HttpGet("{userId}/badge")]
        public async Task<IActionResult> GetUserBadge(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Badge)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                return Ok(new { badgeName = user.Badge?.BadgeName.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

    }
}