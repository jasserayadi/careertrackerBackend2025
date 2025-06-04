using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IRecommendationService recommendationService,
            ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        [HttpGet("user/{userId}/jobs")]
        public async Task<ActionResult<List<JobRecommendation>>> GetRecommendedJobs(int userId)
        {
            try
            {
                var recommendations = await _recommendationService.RecommendJobsForUser(userId);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job recommendations for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/top-jobs")]
        public async Task<ActionResult<List<JobRecommendation>>> GetTopRecommendedJobs(int userId, int count = 3)
        {
            try
            {
                var recommendations = await _recommendationService.RecommendJobsForUser(userId);
                return Ok(recommendations.Take(count).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top job recommendations for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/learning-path")]
        public async Task<ActionResult<LearningPath>> GetLearningPath(int userId)
        {
            try
            {
                var learningPath = await _recommendationService.GetLearningPathAsync(userId);
                return Ok(learningPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learning path for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}