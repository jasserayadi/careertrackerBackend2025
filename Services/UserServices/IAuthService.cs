using Career_Tracker_Backend.Models;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services.UserServices
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        User Authenticate(string username, string password);
     
    }
}