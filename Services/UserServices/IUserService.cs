using Career_Tracker_Backend.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;
using static Career_Tracker_Backend.Services.UserServices.UserService;

namespace Career_Tracker_Backend.Services.UserServices
{
    public interface IUserService
    {
        Task<bool> RegisterUser(string username, string firstname, string lastname,
                                    string password, string confirmPassword, string email,
                                    IFormFile cvFile);

        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> DeleteUserAsync(int userId);

        Task<MoodleCompletionStatus> GetUserCourseCompletionStatusAsync(int userId, int courseId);
        Task<User> GetUserByIdAsync(int userId);


    }
}