// IRecommendationService.cs
using Career_Tracker_Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services
{
    public interface IRecommendationService
    {
        //  Task<List<JobRecommendation>> RecommendJobsForUser(int userId);
        // Task<List<JobRecommendation>> GetRecommendedJobsFromML(int userId, CV cv);
        Task<List<JobRecommendation>> RecommendJobsForUser(int userId);
        Task<(List<string> MatchedSkills, List<string> MissingSkills)> GetSkillGapAsync(int userId, int jobId);
        Task<List<FormationRecommendation>> RecommendFormationsAsync(int userId, List<string> missingSkills);
        Task<LearningPath> GetLearningPathAsync(int userId);
     

    }
}