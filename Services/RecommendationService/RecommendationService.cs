using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Career_Tracker_Backend
{
    public class JobRecommendation
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public float MatchScore { get; set; }
        public List<string> MatchedSkills { get; set; }
        public List<string> MissingSkills { get; set; }
        public string JobDescription { get; set; }
    }

    public class FormationRecommendation
    {
        public int FormationId { get; set; }
        public string Fullname { get; set; }
        public string Summary { get; set; }
        public List<string> MatchedSkills { get; set; }
        public List<string> CourseNames { get; set; }
        public float MatchScore { get; set; }
    }

    public class LearningPath
    {
        public List<string> MatchedSkills { get; set; }
        public List<string> MissingSkills { get; set; }
        public List<FormationRecommendation> RecommendedFormations { get; set; }
    }

    public class FormationRecommendationResponse
    {
        public int FormationId { get; set; }
        public string Fullname { get; set; }
        public string Summary { get; set; }
        public List<string> MatchedSkills { get; set; }
        public float MatchScore { get; set; }
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(
            HttpClient httpClient,
            ApplicationDbContext context,
            ILogger<RecommendationService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;

            // Ensure base address is set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("http://localhost:8000");
            }
        }

        public async Task<List<JobRecommendation>> RecommendJobsForUser(int userId)
        {
            try
            {
                _logger.LogInformation($"Starting job recommendation for user {userId}");

                // Get user's CV with fallback for empty skills/experiences
                var cv = await _context.CVs.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cv == null)
                {
                    _logger.LogWarning($"No CV found for user {userId}");
                    return new List<JobRecommendation>();
                }

                // Ensure skills and experiences are properly deserialized
                var userSkills = cv.Skills ?? new List<string>();
                var userExperiences = cv.Experiences ?? new List<string>();

                _logger.LogInformation($"User skills: {JsonSerializer.Serialize(userSkills)}");
                _logger.LogInformation($"User experiences: {JsonSerializer.Serialize(userExperiences)}");

                // Get all jobs with their required skills
                var jobs = await _context.Jobs
                    .Select(j => new
                    {
                        j.JobId,
                        j.JobName,
                        j.JobDescription,
                        RequiredSkills = j.RequiredSkills ?? new List<string>()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {jobs.Count} jobs to evaluate");

                // Prepare request for ML service
                var request = new
                {
                    user_skills = userSkills,
                    user_experiences = userExperiences,
                    jobs = jobs.Select(j => new
                    {
                        job_id = j.JobId,
                        job_name = j.JobName,
                        required_skills = j.RequiredSkills,
                        job_description = j.JobDescription ?? string.Empty
                    })
                };

                _logger.LogDebug($"Sending request to ML service: {JsonSerializer.Serialize(request)}");

                // Call ML service
                var response = await _httpClient.PostAsJsonAsync("recommend-jobs", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"ML service returned {response.StatusCode}: {errorContent}");
                    return new List<JobRecommendation>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<JobRecommendation>>();

                // Process results
                var recommendations = result?
                    .Where(r => r != null && r.JobId != 0)
                    .OrderByDescending(r => r.MatchScore)
                    .ToList() ?? new List<JobRecommendation>();

                _logger.LogInformation($"Returning {recommendations.Count} job recommendations");

                // Enhance recommendations with additional data
                foreach (var rec in recommendations)
                {
                    var job = jobs.FirstOrDefault(j => j.JobId == rec.JobId);
                    if (job != null)
                    {
                        rec.JobDescription = job.JobDescription;
                        rec.MatchedSkills ??= new List<string>();
                        rec.MissingSkills ??= new List<string>();
                    }
                }

                return recommendations;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error calling ML service for jobs");
                return new List<JobRecommendation>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error parsing ML service response for jobs");
                return new List<JobRecommendation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in job recommendation system");
                return new List<JobRecommendation>();
            }
        }

        public async Task<(List<string> MatchedSkills, List<string> MissingSkills)> GetSkillGapAsync(int userId, int jobId)
        {
            try
            {
                _logger.LogInformation($"Calculating skill gap for user {userId} and job {jobId}");

                var cv = await _context.CVs.FirstOrDefaultAsync(c => c.UserId == userId);
                var userSkills = cv?.Skills ?? new List<string>();

                var job = await _context.Jobs.FindAsync(jobId);
                var jobSkills = job?.RequiredSkills ?? new List<string>();

                var matchedSkills = jobSkills.Intersect(userSkills, StringComparer.OrdinalIgnoreCase).ToList();
                var missingSkills = jobSkills.Except(userSkills, StringComparer.OrdinalIgnoreCase).ToList();

                _logger.LogInformation($"Matched skills: {JsonSerializer.Serialize(matchedSkills)}");
                _logger.LogInformation($"Missing skills: {JsonSerializer.Serialize(missingSkills)}");

                return (matchedSkills, missingSkills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating skill gap");
                return (new List<string>(), new List<string>());
            }
        }

        public async Task<List<FormationRecommendation>> RecommendFormationsAsync(int userId, List<string> missingSkills)
        {
            try
            {
                _logger.LogInformation($"Recommending formations for user {userId} with missing skills: {JsonSerializer.Serialize(missingSkills)}");

                if (!missingSkills.Any())
                {
                    _logger.LogInformation("No missing skills to base recommendations on");
                    return new List<FormationRecommendation>();
                }

                var formations = await _context.Formations
                    .Include(f => f.Courses)
                    .Select(f => new
                    {
                        FormationId = f.FormationId,
                        Fullname = f.Fullname,
                        Summary = f.Summary ?? string.Empty,
                        CourseNames = f.Courses.Select(c => c.Name).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {formations.Count} formations to evaluate");

                var formationRequest = new
                {
                    missing_skills = missingSkills,
                    formations = formations.Select(f => new
                    {
                        formationId = f.FormationId,
                        fullname = f.Fullname,
                        summary = f.Summary
                    }).ToList()
                };

                var response = await _httpClient.PostAsJsonAsync("recommend-formations", formationRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"FastAPI service returned {response.StatusCode}: {errorContent}");
                    return new List<FormationRecommendation>();
                }

                var fastApiResponses = await response.Content.ReadFromJsonAsync<List<FormationRecommendationResponse>>();

                var recommendations = fastApiResponses?
                    .Select(f =>
                    {
                        var formation = formations.FirstOrDefault(fm => fm.FormationId == f.FormationId);
                        return new FormationRecommendation
                        {
                            FormationId = f.FormationId,
                            Fullname = f.Fullname,
                            Summary = f.Summary,
                            MatchedSkills = f.MatchedSkills ?? new List<string>(),
                            CourseNames = formation?.CourseNames ?? new List<string>(),
                            MatchScore = f.MatchScore
                        };
                    })
                    .OrderByDescending(r => r.MatchScore)
                    .ToList() ?? new List<FormationRecommendation>();

                _logger.LogInformation($"Returning {recommendations.Count} formation recommendations");

                return recommendations;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error calling FastAPI for formations");
                return new List<FormationRecommendation>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error parsing FastAPI response for formations");
                return new List<FormationRecommendation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in formation recommendation");
                return new List<FormationRecommendation>();
            }
        }

        public async Task<LearningPath> GetLearningPathAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Generating learning path for user {userId}");

                var user = await _context.Users
                    .Include(u => u.Job)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    _logger.LogWarning($"User {userId} not found");
                    throw new Exception("User not found");
                }

                var learningPath = new LearningPath
                {
                    MatchedSkills = new List<string>(),
                    MissingSkills = new List<string>(),
                    RecommendedFormations = new List<FormationRecommendation>()
                };

                if (user.Job == null)
                {
                    _logger.LogInformation($"No job assigned to user {userId}");
                    return learningPath;
                }

                var (matchedSkills, missingSkills) = await GetSkillGapAsync(userId, user.Job.JobId);
                learningPath.MatchedSkills = matchedSkills;
                learningPath.MissingSkills = missingSkills;

                if (missingSkills.Any())
                {
                    learningPath.RecommendedFormations = await RecommendFormationsAsync(userId, missingSkills);
                }

                _logger.LogInformation($"Learning path generated with {learningPath.RecommendedFormations.Count} formations");

                return learningPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating learning path");
                throw;
            }
        }


    }
}