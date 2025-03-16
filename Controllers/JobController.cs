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
    }
}
