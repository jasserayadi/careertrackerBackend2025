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
            // Retrieve course contents from Moodle
            var courseContents = await _moodleService.GetCourseContentsAsync(moodleCourseId);

            // Use moodleCourseId as FormationId
            int formationId = moodleCourseId;

            // Check if the FormationId exists in the Formations table
            var formationExists = await _context.Formations
                .AnyAsync(f => f.FormationId == formationId);

            if (!formationExists)
            {
                throw new Exception($"Invalid FormationId: The Formation with ID {formationId} does not exist.");
            }

            // Get all existing courses for this Moodle course from our database
            var existingCourses = await _context.Courses
                .Where(c => c.MoodleCourseId == moodleCourseId)
                .ToListAsync();

            // Collect all module identifiers from Moodle
            var moodleModules = new List<(int Section, string Name)>();
            foreach (var content in courseContents)
            {
                foreach (var module in content.Modules)
                {
                    moodleModules.Add((content.Section, module.Name));
                }
            }

            // Identify courses that exist in DB but not in Moodle (deleted content)
            var deletedCourses = existingCourses
                .Where(ec => !moodleModules.Any(mm =>
                    mm.Section == ec.MoodleSectionId &&
                    mm.Name == ec.Name))
                .ToList();

            // Delete courses that no longer exist in Moodle
            if (deletedCourses.Any())
            {
                _context.Courses.RemoveRange(deletedCourses);
            }

            // Add/update existing content
            foreach (var content in courseContents)
            {
                foreach (var module in content.Modules)
                {
                    // Check if the module already exists in the database
                    var existingCourse = existingCourses
                        .FirstOrDefault(c =>
                            c.MoodleCourseId == moodleCourseId &&
                            c.MoodleSectionId == content.Section &&
                            c.Name == module.Name);

                    if (existingCourse == null)
                    {
                        // Create a new Course entity if it doesn't exist
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

                        _context.Courses.Add(course);
                    }
                    else
                    {
                        // Update the existing course if it already exists
                        existingCourse.Summary = content.Summary;
                        existingCourse.Content = module.Contents?.FirstOrDefault()?.Content;
                        existingCourse.Url = module.Url;
                        existingCourse.ModName = module.ModName;
                        existingCourse.ModIcon = module.ModIcon;
                        existingCourse.ModPurpose = module.Purpose;
                        existingCourse.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
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