using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using static Career_Tracker_Backend.Models.DTO;

namespace Career_Tracker_Backend.Services.JobService
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public JobService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Job> CreateJobAsync(Job job)
        {
            // Ajouter un nouveau Job
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<List<JobDto>> GetJobsAsync()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Users) // Include the Users navigation property
                .Select(j => new JobDto
                {
                    JobId = j.JobId,
                    JobName = j.JobName,
                    JobDescription = j.JobDescription,
                    RequiredSkillsJson=j.RequiredSkillsJson,
                    Users = j.Users.Select(u => new UserDto
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Firstname = u.Firstname,
                        Lastname = u.Lastname,
                        Email = u.Email
                    }).ToList()
                })
                .ToListAsync();
            return jobs;
        }

        // In JobService.cs
        public async Task<bool> DeleteJobAsync(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
            {
                return false;
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }
        private bool JobExists(int jobId)
        {
            return _context.Jobs.Any(e => e.JobId == jobId);
        }
        public async Task<Job?> UpdateJobAsync(int jobId, Job jobUpdate)
        {
            var existingJob = await _context.Jobs.FindAsync(jobId);
            if (existingJob == null)
            {
                return null;
            }

            // Only update the fields we want to allow changing
            existingJob.JobName = jobUpdate.JobName;
            existingJob.JobDescription = jobUpdate.JobDescription;
            existingJob.RequiredSkillsJson = jobUpdate.RequiredSkillsJson;


            try
            {
                await _context.SaveChangesAsync();
                return existingJob;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(jobId))
                {
                    return null;
                }
                throw;
            }
        }
        public async Task<List<User>> GetUsersByJobIdAsync(int jobId)
        {
            try
            {
                _logger.LogInformation($"Fetching users for JobId: {jobId}");
                var users = await _context.Users
                    .Where(u => u.JobId == jobId)
                    .Select(u => new User
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Firstname = u.Firstname,
                        Lastname = u.Lastname,
                        Email = u.Email,
                        JobId = u.JobId,
                        Job = u.Job != null ? new Job
                        {
                            JobId = u.Job.JobId,
                            JobName = u.Job.JobName
                        } : null
                    })
                    .ToListAsync();

                if (!users.Any())
                {
                    _logger.LogInformation($"No users found for JobId: {jobId}");
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching users for JobId: {jobId}");
                throw;
            }
        }
        // In JobService.cs
        public async Task<JobDto?> GetJobByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Fetching job for UserId: {userId}");
                var user = await _context.Users
                    .Include(u => u.Job) // Include the Job navigation property
                    .Where(u => u.UserId == userId)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return null;
                }

                if (user.JobId == null || user.Job == null)
                {
                    _logger.LogInformation($"No job associated with UserId: {userId}");
                    return null;
                }

                var job = new JobDto
                {
                    JobId = user.Job.JobId,
                    JobName = user.Job.JobName,
                    JobDescription = user.Job.JobDescription,
                    RequiredSkillsJson = user.Job.RequiredSkillsJson,
                    Users = user.Job.Users.Select(u => new UserDto
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Firstname = u.Firstname,
                        Lastname = u.Lastname,
                        Email = u.Email
                    }).ToList()
                };

                _logger.LogInformation($"Found job {job.JobId} for UserId: {userId}");
                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching job for UserId: {userId}");
                throw;
            }
        }
    }
}