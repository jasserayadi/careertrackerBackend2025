using Career_Tracker_Backend.Models;
using static InscriptionService;

namespace Career_Tracker_Backend.Services.InscriptionService
{
    public interface IInscriptionService
    {
   //     Task SyncEnrollmentsAsync();
       Task UpdateMoodleUserIdsAsync();
        Task SyncEnrollmentsFromMoodleAsync(int courseId);
        //   Task SyncCourseInscriptionsAsync(int moodleCourseId);
        //   Task SyncAllCoursesInscriptionsAsync();
        //  Task<User> GetLocalUserByMoodleIdAsync(int moodleUserId);
        Task<List<InscriptionsByCourseDto>> GetInscriptionsByCourseAsync();
        List<Inscription> GetInscriptionsByCourseId(int courseId);
        List<Formation> GetCoursesByUserId(int userId);
    }
}
