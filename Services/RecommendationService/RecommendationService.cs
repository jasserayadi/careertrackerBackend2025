using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Career_Tracker_Backend.Models;
using Microsoft.Extensions.Logging;

namespace Career_Tracker_Backend
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(HttpClient httpClient, ILogger<RecommendationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<JobRecommendation>> GetJobRecommendationsAsync(int userId, CV cv, List<Job> allJobs)
        {
            try
            {
                _logger.LogInformation($"Starting recommendation for user {userId}");
                _logger.LogInformation($"CV Skills: {string.Join(", ", cv.Skills ?? new List<string>())}");
                _logger.LogInformation($"CV Experiences: {string.Join(", ", cv.Experiences ?? new List<string>())}");
                _logger.LogInformation($"Available Jobs Count: {allJobs.Count}");

                var requestData = new
                {
                    user_id = userId,
                    cv_data = new
                    {
                        skills = cv.Skills ?? new List<string>(),
                        experiences = cv.Experiences ?? new List<string>()
                    },
                    all_jobs = allJobs.Select(j => new
                    {
                        job_id = j.JobId,
                        job_name = j.JobName,
                        job_description = j.JobDescription,
                        required_skills = j.RequiredSkills
                    }).ToList()
                };

                _logger.LogInformation($"Sending to AI service: {JsonSerializer.Serialize(requestData)}");

                var response = await _httpClient.PostAsJsonAsync("recommend-job", requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"AI service error: {response.StatusCode} - {errorContent}");
                    return new List<JobRecommendation>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<JobRecommendation>>()
                    ?? new List<JobRecommendation>();

                _logger.LogInformation($"Received recommendations: {JsonSerializer.Serialize(result)}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations");
                return new List<JobRecommendation>();
            }
        }
    }
    public class JobRecommendation
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public double MatchScore { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
    }
}