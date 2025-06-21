using Career_Tracker_Backend;
using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.InscriptionService;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;

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



   

   public async Task UpdateMoodleUserIdsAsync()
    {
        try
        {
            // Fetch all users from your database
            var users = await _context.Users.ToListAsync();

            // Extract emails to match with Moodle users
            var emails = users.Select(u => u.Email).ToList();

            // Fetch users from Moodle by email
            List<MoodleUser> moodleUsers = new List<MoodleUser>();

            if (emails.Any())
            {
                try
                {
                    moodleUsers = await _moodleService.GetUsersByFieldAsync("email", emails);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching Moodle users by email: {ex.Message}");
                }
            }

            // Update MoodleUserId in your database
            foreach (var user in users)
            {
                // Find the corresponding Moodle user by email
                var moodleUser = moodleUsers.FirstOrDefault(mu =>
                    mu.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));

                if (moodleUser != null)
                {
                    // Update the MoodleUserId
                    user.MoodleUserId = moodleUser.Id;
                    Console.WriteLine($"Updated MoodleUserId for UserId={user.UserId} to {moodleUser.Id} (Email: {user.Email})");
                }
                else
                {
                    Console.WriteLine($"No matching Moodle user found for UserId={user.UserId} (Email: {user.Email})");
                }
            }

            // Save changes to the database
            int changesSaved = await _context.SaveChangesAsync();
            Console.WriteLine($"Saved {changesSaved} changes to the database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during MoodleUserId update: {ex.Message}");
            throw; // Re-throw the exception to propagate it
        }
    }
    public async Task SyncEnrollmentsFromMoodleAsync(int courseId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Fetch enrolled users from Moodle
            var enrolledUsers = await _moodleService.GetEnrolledUsersAsync(courseId);
            _logger.LogInformation($"Retrieved {enrolledUsers.Count} enrolled users for MoodleCourseId: {courseId}");

            // Deduplicate Moodle response by email
            var duplicateEnrollments = enrolledUsers
                .GroupBy(u => u.Email.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            if (duplicateEnrollments.Any())
            {
                _logger.LogWarning($"Duplicate enrollments detected in Moodle response for courseId {courseId}: {string.Join(", ", duplicateEnrollments)}");
                enrolledUsers = enrolledUsers
                    .GroupBy(u => u.Email.ToLower())
                    .Select(g => g.OrderByDescending(u => u.EnrolledSince).First()) // Keep the most recent enrollment
                    .ToList();
            }

            // Fetch the corresponding formation
            var formation = await _context.Formations.FirstOrDefaultAsync(f => f.MoodleCourseId == courseId);
            if (formation == null)
            {
                _logger.LogError($"Formation with MoodleCourseId={courseId} not found in the database.");
                throw new Exception($"Formation with MoodleCourseId={courseId} not found in the database.");
            }

            // Fetch all users
            var users = await _context.Users.ToListAsync();

            // Fetch existing inscriptions for this formation
            var existingInscriptions = await _context.Inscriptions
                .Where(i => i.FormationFk == formation.FormationId)
                .ToListAsync();
            _logger.LogInformation($"Found {existingInscriptions.Count} existing inscriptions for FormationId: {formation.FormationId}");

            // Track processed user IDs to avoid duplicates
            var processedUserIds = new HashSet<int>();

            // Process enrolled users
            foreach (var enrolledUser in enrolledUsers)
            {
                var user = users.FirstOrDefault(u => u.Email.Equals(enrolledUser.Email, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    _logger.LogWarning($"User with email {enrolledUser.Email} not found in the database.");
                    continue;
                }

                if (!processedUserIds.Add(user.UserId))
                {
                    _logger.LogWarning($"User {user.UserId} already processed for FormationId: {formation.FormationId}. Skipping...");
                    continue;
                }

                // Check if the inscription exists, reloading to avoid stale data
                var existingInscription = await _context.Inscriptions
                    .FirstOrDefaultAsync(i => i.UserFk == user.UserId && i.FormationFk == formation.FormationId);

                if (existingInscription == null)
                {
                    // Create a new Inscription
                    var inscription = new Inscription
                    {
                        UserFk = user.UserId,
                        FormationFk = formation.FormationId,
                        InscriptionDate = DateTimeOffset.FromUnixTimeSeconds(enrolledUser.EnrolledSince).DateTime,
                        MoodleEnrollmentId = enrolledUser.Id
                    };
                    _logger.LogInformation($"Adding new inscription for UserId: {user.UserId}, FormationId: {formation.FormationId}");
                    _context.Inscriptions.Add(inscription);
                }
                else
                {
                    // Reload entity to ensure fresh data
                    await _context.Entry(existingInscription).ReloadAsync();
                    existingInscription.InscriptionDate = DateTimeOffset.FromUnixTimeSeconds(enrolledUser.EnrolledSince).DateTime;
                    existingInscription.MoodleEnrollmentId = enrolledUser.Id;
                    _context.Entry(existingInscription).State = EntityState.Modified; // Explicitly mark as modified
                    _logger.LogInformation($"Updating inscription for UserId: {user.UserId}, FormationId: {formation.FormationId}");
                }
            }

            // Identify inscriptions to delete (not in Moodle)
            var enrolledUserEmails = enrolledUsers.Select(u => u.Email.ToLower()).ToHashSet();
            var inscriptionsToDelete = existingInscriptions
                .Where(i => !enrolledUserEmails.Contains(users.FirstOrDefault(u => u.UserId == i.UserFk)?.Email?.ToLower() ?? ""))
                .ToList();

            // Delete inscriptions, reloading to avoid staleness
            foreach (var inscription in inscriptionsToDelete)
            {
                var dbInscription = await _context.Inscriptions
                    .FirstOrDefaultAsync(i => i.InscriptionId == inscription.InscriptionId);
                if (dbInscription != null)
                {
                    _logger.LogInformation($"Deleting inscription: InscriptionId: {dbInscription.InscriptionId}");
                    _context.Inscriptions.Remove(dbInscription);
                }
            }

            // Log change tracker state
            foreach (var entry in _context.ChangeTracker.Entries<Inscription>())
            {
                _logger.LogInformation($"Entity: InscriptionId={entry.Entity.InscriptionId}, State: {entry.State}");
            }

            // Save changes
            int changesSaved = await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation($"Synced {changesSaved} enrollments for MoodleCourseId: {courseId}");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Concurrency error syncing enrollments for MoodleCourseId: {courseId}. Retrying...");
            // Retry once
            await Task.Delay(100); // Brief delay to reduce contention
            await SyncEnrollmentsFromMoodleAsync(courseId); // Recursive retry
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Error syncing enrollments for MoodleCourseId: {courseId}");
            throw;
        }
    }

    public async Task<List<InscriptionsByCourseDto>> GetInscriptionsByCourseAsync()
    {
        // Fetch all inscriptions from the database, including related User and Formation data
        var inscriptions = await _context.Inscriptions
            .Include(i => i.User) // Include User data
            .Include(i => i.Formation) // Include Formation data
            .ToListAsync();

        // Group inscriptions by FormationFk (course ID) and map to DTO
        var inscriptionsByCourse = inscriptions
            .GroupBy(i => i.FormationFk)
            .Select(g => new InscriptionsByCourseDto
            {
                CourseId = g.Key,
                CourseName = g.First().Formation?.Fullname, // Assuming Formation has a Fullname property
                Inscriptions = g.Select(i => new InscriptionDto
                {
                    InscriptionId = i.InscriptionId,
                    InscriptionDate = i.InscriptionDate,
                    UserId = i.UserFk,
                    UserName = i.User?.Username, // Assuming User has a Username property
                    MoodleEnrollmentId = i.MoodleEnrollmentId
                }).ToList()
            })
            .ToList();

        return inscriptionsByCourse;
    }

    public class InscriptionsByCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public List<InscriptionDto> Inscriptions { get; set; }
    }

    public class InscriptionDto
    {
        public int InscriptionId { get; set; }
        public DateTime InscriptionDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int MoodleEnrollmentId { get; set; }
    }
    public InscriptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Inscription> GetInscriptionsByCourseId(int courseId)
    {
        // Query the Inscription table and include related User and Formation data
        var inscriptions = _context.Inscriptions
            .Include(i => i.User) // Include User navigation property
            .Include(i => i.Formation) // Include Formation navigation property
            .Where(i => i.FormationFk == courseId) // Filter by course ID
            .ToList();

        return inscriptions;
    }
    public List<Formation> GetCoursesByUserId(int userId)
    {
        // Query the Inscription table and include related Formation data
        var courses = _context.Inscriptions
            .Include(i => i.Formation) // Include Formation navigation property
            .Where(i => i.UserFk == userId) // Filter by user ID
            .Select(i => i.Formation) // Select the Formation (course) objects
            .Distinct() // Ensure unique courses
            .ToList();

        return courses;
    }
}