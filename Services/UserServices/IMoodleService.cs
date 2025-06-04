
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
        Task<List<MoodleGradeItem>> GetUserGradesAsync(int courseId, int localUserId);
        Task<int?> GetMoodleUserIdAsync(int userId);
        Task<string> GetCourseNameAsync(int courseId);
        Task<string> GetMoodleTokenAsync(string username, string password);
         Task<bool> UpdateMoodleUserAsync(int moodleUserId, string username, string firstname,
            string lastname, string email, string password = null);
        Task<int> CreateMoodleCourseAsync(Formation formation);
      
        Task<int> CreateMoodleQuizAsync(int moodleCourseId, Test test);
        Task<string> GetBookContentAsync(int courseId, int? bookId = null);

        Task<List<MoodleBook>> GetMoodleBooksForCourse(int courseId);
    }
}