using Career_Tracker_Backend.Models;
using static Career_Tracker_Backend.Models.DTO;

namespace Career_Tracker_Backend.Services.JobService
{
    public interface IJobService
    {
        Task<Job> CreateJobAsync(Job job);
        Task<List<JobDto>> GetJobsAsync();
        Task<bool> DeleteJobAsync(int jobId);
        Task<Job?> UpdateJobAsync(int jobId, Job jobUpdate);
        Task<List<User>> GetUsersByJobIdAsync(int jobId);
        Task<JobDto?> GetJobByUserIdAsync(int userId);

    }
}