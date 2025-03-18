using Career_Tracker_Backend.Models;

namespace Career_Tracker_Backend.Services.CourseService
{
    public interface ICourseService
    {
        Task SyncCourseContentsFromMoodleAsync(int moodleCourseId);
        Task<List<Course>> GetCoursesAsync();
        Task<List<Course>> GetCoursesByFormationIdAsync(int formationId);
    }

}