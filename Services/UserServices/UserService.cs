﻿using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public async Task<bool> RegisterUser(string username, string firstname, string lastname, string password, string email, IFormFile cvFile, RoleName role)
        {
            // Vérifier les champs obligatoires
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                return false;

            // Créer un nouvel utilisateur
            var user = new User
            {
                Username = username,
                Firstname = firstname,
                Lastname = lastname,
                Password = password, // Hash the password in a real application
                Email = email,
                DateCreation = DateTimeOffset.UtcNow,
                Role = new Role { RoleName = role } // Assign the role
            };

            // Handle CV upload
            if (cvFile != null && cvFile.Length > 0)
            {
                // Save the CV file to a directory or cloud storage
                var cvFilePath = await SaveCvFileAsync(cvFile);

                // Create a new CV entity
                var cv = new CV
                {
                    CvFile = cvFilePath,
                    User = user
                };

                // Add the CV to the user
                user.CV = cv;
            }

            // Ajouter l'utilisateur à la base de données
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Créer l'utilisateur dans Moodle
            var moodleUserCreated = await _moodleService.CreateMoodleUserAsync(username, firstname, lastname, password, email);
            if (!moodleUserCreated)
            {
                // Gérer l'échec de la création dans Moodle
                throw new Exception("User created locally but failed to create in Moodle.");
            }

            return true;
        }

        private async Task<string> SaveCvFileAsync(IFormFile cvFile)
        {
            // Define the directory to save the CV file
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique file name
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + cvFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file to the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(fileStream);
            }

            // Return only the filename (or relative path)
            return uniqueFileName;
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
                // Find user
                var user = await _context.Users.Include(u => u.CV).FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    throw new Exception("User not found in the local database.");

                // Delete user in Moodle
                var moodleUserDeleted = await _moodleService.DeleteMoodleUserAsync(new List<int> { userId });
                if (!moodleUserDeleted)
                    throw new Exception("Failed to delete user in Moodle.");

                // Delete user's CV file if exists
                if (user.CV != null && !string.IsNullOrEmpty(user.CV.CvFile))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", user.CV.CvFile);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                // Remove user from database
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}");
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