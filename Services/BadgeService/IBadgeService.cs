using Career_Tracker_Backend.Models;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services.Interfaces
{
    public interface IBadgeService
    {
        Task AssignBadgeBasedOnCertificatesAsync(int userId);
        Task AssignBadgesToAllUsersAsync();
    }
}