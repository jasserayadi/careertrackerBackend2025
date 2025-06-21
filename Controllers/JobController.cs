using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.JobService;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Mvc;

namespace Career_Tracker_Backend.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly ILogger<UserService> _logger;

        public JobController(IJobService jobService, ILogger<UserService> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }


      

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] Job job)
        {
            try
            {
                var createdJob = await _jobService.CreateJobAsync(job);
                return Ok(createdJob);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            try
            {
                var jobs = await _jobService.GetJobsAsync();
                return Ok(jobs); // Return the array directly
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // In JobController.cs
        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            try
            {
                var result = await _jobService.DeleteJobAsync(jobId);
                if (!result)
                {
                    return NotFound("Job not found");
                }
                return Ok("Job deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("{jobId}")]
        public async Task<IActionResult> UpdateJob(int jobId, [FromBody] Job jobUpdate)
        {
            try
            {
                // Remove the ID mismatch check since we're getting ID from route
                jobUpdate.JobId = jobId; // Ensure the ID is set from route

                var updatedJob = await _jobService.UpdateJobAsync(jobId, jobUpdate);
                if (updatedJob == null)
                {
                    return NotFound($"Job with ID {jobId} not found");
                }
                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("{jobId}/users")]
        public async Task<IActionResult> GetUsersByJobId(int jobId)
        {
            try
            {
                _logger.LogInformation($"Request received for users with JobId: {jobId}");
                var users = await _jobService.GetUsersByJobIdAsync(jobId);
                if (users.Count == 0)
                {
                    _logger.LogInformation($"No users found for JobId: {jobId}");
                    return NotFound(new { Message = $"No users found for JobId: {jobId}" });
                }
                _logger.LogInformation($"Returning {users.Count} users for JobId: {jobId}");
                return Ok(users);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Invalid JobId: {jobId}");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving users for JobId: {jobId}");
                return StatusCode(500, new { Message = "An error occurred while retrieving users", Error = ex.Message });
            }
        }
        // In JobController.cs
        [HttpGet("users/{userId}/job")]
        public async Task<IActionResult> GetJobByUserId(int userId)
        {
            try
            {
                _logger.LogInformation($"Request received for job with UserId: {userId}");
                var job = await _jobService.GetJobByUserIdAsync(userId);
                if (job == null)
                {
                    _logger.LogInformation($"No job found for UserId: {userId}");
                    return NotFound(new { Message = $"No job found for UserId: {userId}" });
                }
                _logger.LogInformation($"Returning job {job.JobId} for UserId: {userId}");
                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job for UserId: {userId}");
                return StatusCode(500, new { Message = "An error occurred while retrieving the job", Error = ex.Message });
            }
        }
    }
}
