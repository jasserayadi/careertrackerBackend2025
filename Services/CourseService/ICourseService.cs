namespace Career_Tracker_Backend.Services.CourseService
{
    public interface ICourseService
    {
        Task SyncCourseContentsFromMoodleAsync(int moodleCourseId);
    }
}