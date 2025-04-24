// ICvService.cs
using Career_Tracker_Backend.Models;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services
{
    public interface ICvService
    {
        Task<CV> ExtractFromStoredCvAsync(CV cv);
    }
}