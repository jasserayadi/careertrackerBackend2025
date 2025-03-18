using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.InscriptionService;
using Career_Tracker_Backend.Services.UserServices;
using Career_Tracker_Backend;
using Microsoft.EntityFrameworkCore;

public class InscriptionService : IInscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMoodleService _moodleService;
    private readonly ILogger<InscriptionService> _logger;
    public InscriptionService(ApplicationDbContext context, IMoodleService moodleService, ILogger<InscriptionService> logger)
    {
        _context = context;
        _moodleService = moodleService;
        _logger = logger;
    }
    public async Task SyncEnrollmentsAsync(int courseId)
    {
        // Fetch enrolled users from Moodle
        var enrolledUsers = await _moodleService.GetEnrolledUsersAsync(courseId);

        _logger.LogInformation("Fetched {Count} enrolled users from Moodle.", enrolledUsers.Count); // Log the number of users

        foreach (var moodleUser in enrolledUsers)
        {
            foreach (var moodleCourse in moodleUser.EnrolledCourses)
            {
                // Find the corresponding Formation in your database
                var formation = await _context.Formations
                    .FirstOrDefaultAsync(f => f.MoodleCourseId == moodleCourse.Id);

                // Find the corresponding User in your database
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.MoodleUserId == moodleUser.Id);

                if (formation != null && user != null)
                {
                    // Check if the enrollment already exists in your database
                    var existingInscription = await _context.Inscriptions
                        .FirstOrDefaultAsync(i =>
                            i.MoodleEnrollmentId == moodleCourse.Id &&
                            i.User.UserId == user.UserId);

                    if (existingInscription == null)
                    {
                        // Create a new inscription if it doesn't exist
                        var inscription = new Inscription
                        {
                            InscriptionDate = DateTimeOffset.FromUnixTimeSeconds(moodleCourse.TimeCreated).DateTime, // Map TimeCreated to InscriptionDate
                            User = user, // Map Moodle user to User
                            Formation = formation, // Map Moodle course to Formation
                            MoodleEnrollmentId = moodleCourse.Id // Map Moodle enrollment ID
                        };

                        // Add the inscription to the database
                        _context.Inscriptions.Add(inscription);
                        _logger.LogInformation("Added new inscription for User ID {UserId} and Formation ID {FormationId}.", user.UserId, formation.FormationId);
                    }
                    else
                    {
                        // Update the existing inscription
                        existingInscription.InscriptionDate = DateTimeOffset.FromUnixTimeSeconds(moodleCourse.TimeCreated).DateTime;

                        _logger.LogInformation("Updated existing inscription for User ID {UserId} and Formation ID {FormationId}.", user.UserId, formation.FormationId);
                    }
                }
                else
                {
                    _logger.LogWarning("User or Formation not found for Moodle User ID {MoodleUserId} and Moodle Course ID {MoodleCourseId}.", moodleUser.Id, moodleCourse.Id);
                }
            }
        }

        // Save changes to the database
        await _context.SaveChangesAsync();
        _logger.LogInformation("Saved changes to the database.");
    }

    /* public async Task SyncEnrollmentsAsync(int courseId)
     {
         // Fetch enrolled users and their enrollments from Moodle
         var enrolledUsers = await _moodleService.GetEnrolledUsersAsync(courseId);

         foreach (var moodleUser in enrolledUsers)
         {
             foreach (var moodleCourse in moodleUser.EnrolledCourses)
             {
                 // Check if the enrollment already exists in your database
                 var existingInscription = await _context.Inscriptions
                     .FirstOrDefaultAsync(i =>
                         i.MoodleEnrollmentId == moodleCourse.Id &&
                         i.User.MoodleUserId == moodleUser.Id);

                 if (existingInscription == null)
                 {
                     // Create a new inscription if it doesn't exist
                     var inscription = new Inscription
                     {
                         InscriptionDate = DateTimeOffset.FromUnixTimeSeconds(moodleCourse.TimeCreated).DateTime, // Convert Unix timestamp to DateTime
                         User = await _context.Users.FirstOrDefaultAsync(u => u.MoodleUserId == moodleUser.Id), // Find the user in your database
                         Formation = await _context.Formations.FirstOrDefaultAsync(f => f.MoodleCourseId == moodleCourse.Id), // Find the formation in your database
                         MoodleEnrollmentId = moodleCourse.Id // Store Moodle's enrollment ID
                     };

                     // Add the inscription to the database
                     _context.Inscriptions.Add(inscription);
                 }
                 else
                 {
                     // Update the existing inscription
                     existingInscription.InscriptionDate = DateTimeOffset.FromUnixTimeSeconds(moodleCourse.TimeCreated).DateTime;
                     existingInscription.UpdatedAt = DateTime.UtcNow;
                 }
             }
         }

         // Save changes to the database
         await _context.SaveChangesAsync();
     }*/
}