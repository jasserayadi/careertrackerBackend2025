using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.FormationService;
using Career_Tracker_Backend.Services.UserServices;
using Career_Tracker_Backend;
using Microsoft.EntityFrameworkCore;

public class FormationService : IFormationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMoodleService _moodleService;

    public FormationService(ApplicationDbContext context, IMoodleService moodleService)
    {
        _context = context;
        _moodleService = moodleService;
    }

    public async Task SyncFormationsAsync()
    {
        // Fetch courses from Moodle
        var moodleCourses = await _moodleService.GetCoursesAsync();

        foreach (var moodleCourse in moodleCourses)
        {
            // Check if the course already exists in the database
            var existingFormation = await _context.Formations
                .FirstOrDefaultAsync(f => f.MoodleCourseId == moodleCourse.Id);

            if (existingFormation == null)
            {
                // Map Moodle course to Formation entity
                var formation = new Formation
                {
                    Fullname = moodleCourse.Fullname,
                    Shortname = moodleCourse.Shortname,
                    Summary = moodleCourse.Summary,
                    MoodleCategoryId = moodleCourse.Categoryid,
                    MoodleCourseId = moodleCourse.Id,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to database
                _context.Formations.Add(formation);
            }
            else
            {
                // Update existing formation
                existingFormation.Fullname = moodleCourse.Fullname;
                existingFormation.Shortname = moodleCourse.Shortname;
                existingFormation.Summary = moodleCourse.Summary;
                existingFormation.MoodleCategoryId = moodleCourse.Categoryid;
                existingFormation.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Save changes
        await _context.SaveChangesAsync();
    }
}