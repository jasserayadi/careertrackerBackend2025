using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using static Career_Tracker_Backend.Models.DTO;

namespace Career_Tracker_Backend.Services.JobService
{
    public class JobService:IJobService
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

        /*public async Task<List<User>> GetUsersAsync()
{
   var users = await _context.Users
       .Include(u => u.CV)
       .Include(u => u.Role)
       .ToListAsync();
   return users;
}*/

    }
}
