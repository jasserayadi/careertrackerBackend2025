
using Career_Tracker_Backend.Models;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;

namespace Career_Tracker_Backend.Services.UserServices
{
    public interface IMoodleService
    {
        Task<bool> CreateMoodleUserAsync(string username, string firstname, string lastname, string password, string email);
        Task<bool> DeleteMoodleUserAsync(List<int> userIds);
        Task<List<MoodleCourse>> GetCoursesAsync();
       Task<List<MoodleCourseContent>> GetCourseContentsAsync(int courseId);
             Task SaveQuizDataAsync(int courseId, int userId);
         Task<List<MoodleQuiz>> GetQuizzesByCourseAsync(int courseId);
        QuizQuestionDetail ParseHtmlContent(string htmlContent);
        Task<List<MoodleEnrolledUser>> GetEnrolledUsersAsync(int courseId);
        Task<List<MoodleUser>> GetUsersByFieldAsync(string field, List<string> values);
        Task<MoodleCompletionStatus> GetCourseCompletionStatusAsync(int userId, int courseId);
          }
}