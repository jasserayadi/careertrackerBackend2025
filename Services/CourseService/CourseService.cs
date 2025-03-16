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

        public CourseService(ApplicationDbContext context, IMoodleService moodleService)
        {
            _context = context;
            _moodleService = moodleService;
        }

        public async Task SyncCourseContentsFromMoodleAsync(int moodleCourseId)
        {
            // Retrieve course contents from Moodle
            var courseContents = await _moodleService.GetCourseContentsAsync(moodleCourseId);

            foreach (var content in courseContents)
            {
                // Use moodleCourseId as FormationId
                int formationId = moodleCourseId;

                // Check if the FormationId exists in the Formations table
                var formationExists = await _context.Formations
                    .AnyAsync(f => f.FormationId == formationId);

                if (!formationExists)
                {
                    throw new Exception($"Invalid FormationId: The Formation with ID {formationId} does not exist.");
                }

                // Loop through each module in the course content
                foreach (var module in content.Modules)
                {
                    // Create a new Course entity
                    var course = new Course
                    {
                        Name = module.Name, // Module name (e.g., "Announcements")
                        Summary = content.Summary, // Section summary (if available)
                        Content = module.Contents?.FirstOrDefault()?.Content, // Module content (if available)
                        MoodleCourseId = moodleCourseId,
                        MoodleSectionId = content.Section,
                        Url = module.Url, // Module URL (e.g., "http://localhost/Mymoodle/mod/forum/view.php?id=2")
                        ModName = module.ModName, // Module type (e.g., "forum")
                        ModIcon = module.ModIcon, // Module icon URL (e.g., "http://localhost/Mymoodle/theme/image.php/boost/forum/1741792922/monologo?filtericon=1")
                        ModPurpose = module.Purpose, // Module purpose (e.g., "collaboration")
                        FormationId = formationId, // Use moodleCourseId as FormationId
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Add the Course to the database
                    _context.Courses.Add(course);
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

    }
}