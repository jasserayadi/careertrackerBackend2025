using Career_Tracker_Backend.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services.UserServices
{
    public interface IUserService
    {
        Task<bool> RegisterUser(string username, string firstname, string lastname, string password, string email, IFormFile cvFile, RoleName role);
        Task<List<User>> GetUsersAsync();
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> DeleteUserAsync(int userId);
    }
}