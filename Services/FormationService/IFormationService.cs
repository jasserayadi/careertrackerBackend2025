using Career_Tracker_Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Career_Tracker_Backend.Services.FormationService
{
    public interface IFormationService
    {
   
         Task SyncFormationsAsync();
      //  Task<int> CreateFullFormationAsync(Formation formation);
    }
}
