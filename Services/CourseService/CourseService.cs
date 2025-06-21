using System;
using System.Threading.Tasks;
using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.EntityFrameworkCore;

namespace Career_Tracker_Backend.Services.CourseService
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMoodleService _moodleService;
        private readonly ILogger<CourseService> _logger;

        public CourseService(ApplicationDbContext context, IMoodleService moodleService, ILogger<CourseService> logger)
        {
            _context = context;
            _moodleService = moodleService;
            _logger = logger;
        }

        public async Task SyncCourseContentsFromMoodleAsync(int moodleCourseId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Retrieve course contents from Moodle
                var courseContents = await _moodleService.GetCourseContentsAsync(moodleCourseId);
                _logger.LogInformation($"Retrieved {courseContents.Count} sections for MoodleCourseId: {moodleCourseId}");

                // Use moodleCourseId as FormationId
                int formationId = moodleCourseId;

                // Check if the FormationId exists
                var formationExists = await _context.Formations.AnyAsync(f => f.FormationId == formationId);
                if (!formationExists)
                {
                    _logger.LogError($"Formation with ID {formationId} does not exist.");
                    throw new Exception($"Invalid FormationId: The Formation with ID {formationId} does not exist.");
                }

                // Get all existing courses for this Moodle course
                var existingCourses = await _context.Courses
                    .Where(c => c.MoodleCourseId == moodleCourseId)
                    .ToListAsync();

                // Check for duplicates in the database
                var duplicateCourses = existingCourses
                    .GroupBy(c => new { c.MoodleCourseId, c.MoodleSectionId, NormalizedName = c.Name?.Trim().ToLower() })
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.Skip(1)) // Keep the first record, mark others for deletion
                    .ToList();

                if (duplicateCourses.Any())
                {
                    _logger.LogWarning($"Found {duplicateCourses.Count} duplicate courses for MoodleCourseId: {moodleCourseId}. Removing duplicates...");
                    _context.Courses.RemoveRange(duplicateCourses);
                    await _context.SaveChangesAsync();
                }

                // Collect all module identifiers from Moodle, ensuring uniqueness
                var moodleModules = new HashSet<(int Section, string NormalizedName)>();
                foreach (var content in courseContents)
                {
                    foreach (var module in content.Modules)
                    {
                        var normalizedName = module.Name?.Trim().ToLower();
                        if (!string.IsNullOrEmpty(normalizedName))
                        {
                            var moduleKey = (content.Section, normalizedName);
                            if (!moodleModules.Add(moduleKey))
                            {
                                _logger.LogWarning($"Duplicate module detected in Moodle response: Section {content.Section}, Name: {module.Name}");
                            }
                        }
                    }
                }

                // Identify courses that exist in DB but not in Moodle
                var deletedCourses = existingCourses
                    .Where(ec => !moodleModules.Any(mm =>
                        mm.Section == ec.MoodleSectionId &&
                        mm.NormalizedName == ec.Name?.Trim().ToLower()))
                    .ToList();

                // Delete courses that no longer exist in Moodle
                if (deletedCourses.Any())
                {
                    _logger.LogInformation($"Deleting {deletedCourses.Count} courses not found in Moodle for MoodleCourseId: {moodleCourseId}");
                    _context.Courses.RemoveRange(deletedCourses);
                }

                // Add/update existing content
                foreach (var content in courseContents)
                {
                    foreach (var module in content.Modules)
                    {
                        var normalizedName = module.Name?.Trim().ToLower();
                        if (string.IsNullOrEmpty(normalizedName)) continue;

                        // Check if the module already exists
                        var existingCourse = existingCourses
                            .FirstOrDefault(c =>
                                c.MoodleCourseId == moodleCourseId &&
                                c.MoodleSectionId == content.Section &&
                                c.Name?.Trim().ToLower() == normalizedName);

                        if (existingCourse == null)
                        {
                            // Create new Course entity
                            var course = new Course
                            {
                                Name = module.Name,
                                Summary = content.Summary,
                                Content = module.Contents?.FirstOrDefault()?.Content,
                                MoodleCourseId = moodleCourseId,
                                MoodleSectionId = content.Section,
                                Url = module.Url,
                                ModName = module.ModName,
                                ModIcon = module.ModIcon,
                                ModPurpose = module.Purpose,
                                FormationId = formationId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _logger.LogInformation($"Adding new course: {course.Name} (Section: {course.MoodleSectionId}, MoodleCourseId: {moodleCourseId})");
                            _context.Courses.Add(course);
                            existingCourses.Add(course); // Update in-memory list to avoid re-processing
                        }
                        else
                        {
                            // Update existing course
                            existingCourse.Summary = content.Summary;
                            existingCourse.Content = module.Contents?.FirstOrDefault()?.Content;
                            existingCourse.Url = module.Url;
                            existingCourse.ModName = module.ModName;
                            existingCourse.ModIcon = module.ModIcon;
                            existingCourse.ModPurpose = module.Purpose;
                            existingCourse.UpdatedAt = DateTime.UtcNow;
                            _logger.LogInformation($"Updating course: {existingCourse.Name} (Section: {existingCourse.MoodleSectionId}, MoodleCourseId: {moodleCourseId})");
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation($"Course contents synchronized successfully for MoodleCourseId: {moodleCourseId}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error synchronizing course contents for MoodleCourseId: {moodleCourseId}");
                throw;
            }
        }
        public async Task<List<Course>> GetCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByFormationIdAsync(int formationId)
        {
            return await _context.Courses
                .Where(c => c.FormationId == formationId)
                .ToListAsync();
        }

    }



}