using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Career_Tracker_Backend.Services
{
    public class CvService : ICvService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CvService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public CvService(
            HttpClient httpClient,
            ILogger<CvService> logger,
            IWebHostEnvironment env,
            ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = env;
            _context = context;
        }

        public async Task<CV> ExtractFromStoredCvAsync(CV cv)
        {
            try
            {
                if (string.IsNullOrEmpty(cv.CvFile))
                {
                    _logger.LogWarning("CV file path is empty");
                    return cv;
                }

                // Remove leading slash if present
                var cleanPath = cv.CvFile.TrimStart('/');
                var filePath = Path.Combine(_env.WebRootPath, "uploads", cleanPath);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("CV file not found at {Path}", filePath);
                    return cv;
                }

                // Process with ML service
                await using var fileStream = File.OpenRead(filePath);
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(fileContent, "file", Path.GetFileName(filePath));

                var response = await _httpClient.PostAsync("http://localhost:8000/extract-from-cv", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ML service error: {StatusCode} - {Error}",
                        response.StatusCode, error);
                    return cv;
                }

                var result = await response.Content.ReadFromJsonAsync<ExtractionResult>();

                // Update CV with extracted data
                cv.Skills = result?.Skills ?? new List<string>();
                cv.Experiences = result?.Experiences ?? new List<string>();

                // Convert to JSON and store in database fields
                cv.SkillsJson = JsonSerializer.Serialize(cv.Skills);
                cv.ExperiencesJson = JsonSerializer.Serialize(cv.Experiences);

                // Attach and update
                if (_context.Entry(cv).State == EntityState.Detached)
                {
                    _context.CVs.Attach(cv);
                }
                _context.Entry(cv).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return cv;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CV");
                throw;
            }
        }

        private record ExtractionResult(List<string> Skills, List<string> Experiences);
    }
}