using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.UserServices;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;
using static QuizService;

namespace Career_Tracker_Backend.Services.QuizService
{
    public interface IQuizService
    {
        Task<List<QuizQuestionDetail>> GetTestQuestionsAsync(int testId);
        Task<List<Test>> GetTestsByCourseAndMoodleQuizIdAsync(int courseId, int? moodleQuizId = null);
    }
}
