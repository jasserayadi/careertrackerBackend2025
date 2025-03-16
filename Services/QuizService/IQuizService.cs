using Career_Tracker_Backend.Services.UserServices;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;

namespace Career_Tracker_Backend.Services.QuizService
{
    public interface IQuizService
    {
        Task<List<QuizQuestionDetail>> GetTestQuestionsAsync(int testId);
    }
}
