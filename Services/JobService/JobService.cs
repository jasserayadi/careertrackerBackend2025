using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using static Career_Tracker_Backend.Models.DTO;

namespace Career_Tracker_Backend.Services.JobService
{
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;

        public JobService(ApplicationDbContext context)
        {
            _context = context;
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

    }
}