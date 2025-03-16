using Microsoft.AspNetCore.Mvc;
using Career_Tracker_Backend.Services.UserServices;
using Career_Tracker_Backend.Models;
using System.Threading.Tasks;
using Career_Tracker_Backend.Services.FormationService;
using Microsoft.EntityFrameworkCore;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormationsController : ControllerBase
    {
        private readonly IFormationService _formationService;
        private readonly ApplicationDbContext _context; // Add this line

        public FormationsController(IFormationService formationService, ApplicationDbContext context) // Add ApplicationDbContext here
        {
            _formationService = formationService;
            _context = context; // Initialize the context
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncFormations()
        {
            try
            {
                await _formationService.SyncFormationsAsync();
                return Ok("Formations synced successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFormations()
        {
            var formations = await _context.Formations.ToListAsync(); // Now _context is defined
            return Ok(formations);
        }
    }
}