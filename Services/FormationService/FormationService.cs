using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.FormationService;
using Career_Tracker_Backend.Services.UserServices;
using Career_Tracker_Backend;
using Microsoft.EntityFrameworkCore;

public class FormationService : IFormationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMoodleService _moodleService;
    private readonly ILogger<FormationService> _logger;

    public FormationService(ApplicationDbContext context, IMoodleService moodleService, ILogger<FormationService> logger)
    {
        _context = context;
        _moodleService = moodleService;
        _logger = logger;
    }

    public async Task SyncFormationsAsync()
    {
        // Fetch current courses from Moodle
        var moodleCourses = await _moodleService.GetCoursesAsync();
        var moodleCourseIds = moodleCourses.Select(c => c.Id).ToList();

        // Get all formations from database
        var dbFormations = await _context.Formations.ToListAsync();

        // Identify formations that exist in DB but not in Moodle (deleted courses)
        var deletedFormationIds = dbFormations
            .Where(f => !moodleCourseIds.Contains(f.MoodleCourseId))
            .Select(f => f.FormationId)
            .ToList();

        // Delete formations that no longer exist in Moodle
        if (deletedFormationIds.Any())
        {
            var formationsToDelete = dbFormations
                .Where(f => deletedFormationIds.Contains(f.FormationId))
                .ToList();

            _context.Formations.RemoveRange(formationsToDelete);
        }

        // Add/update existing courses
        foreach (var moodleCourse in moodleCourses)
        {
            var existingFormation = dbFormations
                .FirstOrDefault(f => f.MoodleCourseId == moodleCourse.Id);

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

        await _context.SaveChangesAsync();
    }
   /* public async Task<int> CreateFullFormationAsync(Formation formation)
    {
        try
        {
            _logger.LogInformation($"Starting creation of full formation: {formation.Fullname}");

            // 1. First create the Moodle course (your Formation)
            var moodleCourseId = await _moodleService.CreateMoodleCourseAsync(formation);

            // 2. Create sections (your Courses) within this Moodle course
            if (formation.Courses != null && formation.Courses.Any())
            {
                foreach (var course in formation.Courses)
                {
                    // Create course section in Moodle
                    var sectionId = await _moodleService.CreateCourseSectionAsync(moodleCourseId, course);
                    course.MoodleSectionId = sectionId;

                    // 3. Create quizzes (your Tests) for this course
                    if (course.Test != null)
                    {
                        var quizId = await _moodleService.CreateMoodleQuizAsync(moodleCourseId, course.Test);
                        course.Test.MoodleQuizId = quizId;

                        // 4. Add questions to the quiz
                        if (course.Test.Questions != null && course.Test.Questions.Any())
                        {
                            await _moodleService.AddQuestionsToQuizAsync(quizId, course.Test.Questions);
                        }
                    }
                }
            }

            _logger.LogInformation($"Successfully created formation in Moodle with ID: {moodleCourseId}");
            return moodleCourseId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create full formation in Moodle");
            throw;
        }
    }*/
}