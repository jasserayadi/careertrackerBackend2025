// Controllers/RecommendationController.cs
using Microsoft.AspNetCore.Mvc;
using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Career_Tracker_Backend.Services.RecommendationService;
using Microsoft.EntityFrameworkCore;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationController(RecommendationService recommendationService, ApplicationDbContext context, ILogger<RecommendationService> logger)
        {
            _recommendationService = recommendationService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<JobRecommendation>>> GetRecommendationsForUser(int userId)
        {
            try
            {
                // Get user with CV
                var user = await _context.Users
                    .Include(u => u.CV)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user?.CV == null)
                {
                    return NotFound("User or CV not found");
                }

                // Get all jobs first (since we can't filter RequiredSkills in SQL)
                var allJobs = await _context.Jobs.ToListAsync();

                // Then filter in memory for jobs with required skills
                var jobsWithSkills = allJobs
                    .Where(j => j.RequiredSkills != null && j.RequiredSkills.Any())
                    .ToList();

                if (!jobsWithSkills.Any())
                {
                    return NotFound("No jobs with required skills available");
                }

                // Get recommendations
                var recommendations = await _recommendationService
                    .GetJobRecommendationsAsync(userId, user.CV, jobsWithSkills);

                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recommendations for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
    }