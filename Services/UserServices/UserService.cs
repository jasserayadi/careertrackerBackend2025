using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;

namespace Career_Tracker_Backend.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMoodleService _moodleService;
        private readonly ILogger<UserService> _logger;
        public UserService(ApplicationDbContext context, IMoodleService moodleService, ILogger<UserService> logger)
        {
            _context = context;
            _moodleService = moodleService;
            _logger = logger;
        }

        public async Task<bool> RegisterUser(string username, string firstname, string lastname,
                                    string password, string confirmPassword, string email,
                                    IFormFile cvFile)
        {
            // Check required fields
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(confirmPassword))
            {
                throw new Exception("All required fields must be filled.");
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                throw new Exception("Password and confirmation password do not match.");
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                throw new Exception("Email already exists.");
            }

            // Validate password complexity
            if (!IsPasswordValid(password))
            {
                throw new Exception("Password must have at least 8 characters, including 1 digit, " +
                                  "1 lowercase, 1 uppercase, and 1 special character.");
            }

            // Create new user with hardcoded NewEmploye role
            var user = new User
            {
                Username = username,
                Firstname = firstname,
                Lastname = lastname,
                Password = HashPassword(password), // Always hash passwords before storing
                Email = email,
                DateCreation = DateTimeOffset.UtcNow,
                Role = new Role { RoleName = RoleName.NewEmploye } // Hardcoded role
            };

            // Handle CV upload
            if (cvFile != null && cvFile.Length > 0)
            {
                var cvFilePath = await SaveCvFileAsync(cvFile);
                var cv = new CV
                {
                    CvFile = cvFilePath,
                    User = user
                };
                user.CV = cv;
            }
            else
            {
                throw new Exception("CV file is required.");
            }

            // Add user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create user in Moodle
            try
            {
                var moodleUserCreated = await _moodleService.CreateMoodleUserAsync(username, firstname, lastname, password, email);
                if (!moodleUserCreated)
                {
                    // Optional: You might want to delete the locally created user if Moodle creation fails
                    throw new Exception("User created locally but failed to create in Moodle.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Moodle user creation failed: {ex.Message}");
            }

            return true;
        }

        private bool IsPasswordValid(string password)
        {
            // Password must have:
            // - at least 8 characters
            // - at least 1 digit
            // - at least 1 lowercase letter
            // - at least 1 uppercase letter
            // - at least 1 special character (*, -, #, etc.)
            var regex = new Regex(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$");
            return regex.IsMatch(password);
        }

        private string HashPassword(string password)
        {
            // Example using BCrypt (you'll need to install the BCrypt.Net-Next package)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private async Task<string> SaveCvFileAsync(IFormFile cvFile)
        {
            // Ensure the uploads directory exists
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique file name
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + cvFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(fileStream);
            }

            return $"/{uniqueFileName}"; // Return the relative path
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.CV)
                .Include(u => u.Role)
                .ToListAsync();
            return users;
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            // Récupérer l'utilisateur par son nom d'utilisateur
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            return user;
        }
        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find the user with related entities
                var user = await _context.Users
                    .Include(u => u.CV)
                    .Include(u => u.Inscriptions)
                    .Include(u => u.Feedbacks)
                    .Include(u => u.Certificats)
                    
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    throw new Exception("User not found in the local database.");

                // Ensure MoodleUserId exists before trying to delete it on Moodle
                if (!user.MoodleUserId.HasValue)
                    throw new Exception("User has no associated Moodle account");

                // Call Moodle deletion using MoodleUserId
                var moodleUserDeleted = await _moodleService.DeleteMoodleUserAsync(
                    new List<int> { user.MoodleUserId.Value });

                if (!moodleUserDeleted)
                    throw new Exception("Failed to delete user in Moodle.");

                // Clean up related Inscriptions
                if (user.Inscriptions != null && user.Inscriptions.Any())
                {
                    _context.Inscriptions.RemoveRange(user.Inscriptions);
                }

                // Clean up related Feedbacks
                if (user.Feedbacks != null && user.Feedbacks.Any())
                {
                    _context.Feedbacks.RemoveRange(user.Feedbacks);
                }

                // Clean up related Certificates
                if (user.Certificats != null && user.Certificats.Any())
                {
                    _context.Certificats.RemoveRange(user.Certificats);
                }

                // Remove from Formations if needed (depends on relationship setup)
                if (user.Formations != null && user.Formations.Any())
                {
                    foreach (var formation in user.Formations.ToList())
                    {
                        user.Formations.Remove(formation);
                    }
                }

                // Remove CV and file if exists
                if (user.CV != null)
                {
                    if (!string.IsNullOrEmpty(user.CV.CvFile))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                            "wwwroot", "uploads", user.CV.CvFile);
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    _context.CVs.Remove(user.CV);
                }

                // Remove the user itself
                _context.Users.Remove(user);

                // Save and commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error deleting user {userId}: {ex.Message}");
                return false;
            }
        }






        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users

                .Include(u => u.Role)
                .Include(u => u.CV)




                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return user;
        }
        public async Task<MoodleCompletionStatus> GetUserCourseCompletionStatusAsync(int userId, int courseId)
        {
            return await _moodleService.GetCourseCompletionStatusAsync(userId, courseId);
        }

    }

}