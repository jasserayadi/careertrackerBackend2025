



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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Fetch current courses from Moodle
            var moodleCourses = await _moodleService.GetCoursesAsync();
            var moodleCourseIds = moodleCourses.Select(c => c.Id).Distinct().ToList(); // Ensure unique Moodle course IDs

            // Log Moodle courses for debugging
            _logger.LogInformation($"Retrieved {moodleCourses.Count} courses from Moodle: {string.Join(", ", moodleCourseIds)}");

            // Get all formations from database
            var dbFormations = await _context.Formations.ToListAsync();

            // Check for duplicate MoodleCourseIds in the database
            var duplicateFormations = dbFormations
                .GroupBy(f => f.MoodleCourseId)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Skip(1)) // Keep the first record, mark others for deletion
                .ToList();

            if (duplicateFormations.Any())
            {
                _logger.LogWarning($"Found {duplicateFormations.Count} duplicate formations in the database. Removing duplicates...");
                _context.Formations.RemoveRange(duplicateFormations);
                await _context.SaveChangesAsync();
            }

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
                _logger.LogInformation($"Deleting {formationsToDelete.Count} formations not found in Moodle.");
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
                    _logger.LogInformation($"Adding new formation: {formation.Fullname} (MoodleCourseId: {formation.MoodleCourseId})");
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
                    _logger.LogInformation($"Updating formation: {existingFormation.Fullname} (MoodleCourseId: {existingFormation.MoodleCourseId})");
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation("Formations synchronized successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during formation synchronization");
            throw;
        }
    }
    public async Task DeleteFormationAndMoodleCourseAsync(int formationId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Load the formation with its related Courses and Inscriptions
            var formation = await _context.Formations
                .Include(f => f.Courses)
                .Include(f => f.Inscriptions)
                .FirstOrDefaultAsync(f => f.FormationId == formationId);

            if (formation == null)
            {
                _logger.LogWarning($"Formation with ID {formationId} not found.");
                throw new Exception($"Formation with ID {formationId} not found.");
            }

            // Delete the Moodle course
            await _moodleService.DeleteMoodleCourseAsync(formation.MoodleCourseId);

            // Delete related inscriptions from the database
            if (formation.Inscriptions != null && formation.Inscriptions.Any())
            {
                _context.Inscriptions.RemoveRange(formation.Inscriptions);
                _logger.LogInformation($"Deleted {formation.Inscriptions.Count} inscriptions for formation ID {formationId}");
            }

            // Delete related courses from the database
            if (formation.Courses != null && formation.Courses.Any())
            {
                _context.Courses.RemoveRange(formation.Courses);
                _logger.LogInformation($"Deleted {formation.Courses.Count} courses for formation ID {formationId}");
            }

            // Delete the formation from the database
            _context.Formations.Remove(formation);
            _logger.LogInformation($"Deleted formation with ID {formationId}");

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();

            _logger.LogInformation($"Successfully deleted formation with ID {formationId}, its {formation.Inscriptions?.Count ?? 0} inscriptions, {formation.Courses?.Count ?? 0} courses, and Moodle course with ID {formation.MoodleCourseId}");
        }
        catch (Exception ex)
        {
            // Roll back the transaction on failure
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Failed to delete formation with ID {formationId}, its inscriptions, courses, and Moodle course.");
            throw;
        }
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







