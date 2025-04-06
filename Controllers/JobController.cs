using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.JobService;
using Microsoft.AspNetCore.Mvc;

namespace Career_Tracker_Backend.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
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
    }
}
