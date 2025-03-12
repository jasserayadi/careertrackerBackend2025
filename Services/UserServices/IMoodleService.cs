
namespace Career_Tracker_Backend.Services.UserServices
{
    public interface IMoodleService
    {
        Task<bool> CreateMoodleUserAsync(string username, string firstname, string lastname, string password, string email);
        Task<bool> DeleteMoodleUserAsync(List<int> list);
        Task<int?> GetMoodleUserIdByEmailAsync(string email);
    }
}