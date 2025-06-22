using Career_Tracker_Backend.Models;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services.UserServices
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        User Authenticate(string username, string password);
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword, string confirmPassword);
        Task<bool> ValidateResetTokenAsync(string token);

    }
}