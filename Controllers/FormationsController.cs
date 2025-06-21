using Microsoft.AspNetCore.Mvc;
using Career_Tracker_Backend.Services.UserServices;
using Career_Tracker_Backend.Models;
using System.Threading.Tasks;
using Career_Tracker_Backend.Services.FormationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormationsController : ControllerBase
    {
        private readonly IFormationService _formationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FormationsController> _logger;

        public FormationsController(
            IFormationService formationService,
            ApplicationDbContext context,
            ILogger<FormationsController> logger)
        {
            _formationService = formationService;
            _context = context;
            _logger = logger;
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
                _logger.LogError(ex, "Error syncing formations");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFormations()
        {
            try
            {
                var formations = await _context.Formations.ToListAsync();
                return Ok(formations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting formations");
                return StatusCode(500, "Internal server error");
            }
        }

        /*[HttpPost]
                public async Task<IActionResult> CreateFullFormation([FromBody] Formation formation)
                {
                    try
                    {
                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        var moodleCourseId = await _formationService.CreateFullFormationAsync(formation);

                        if (formation.FormationId == 0)
                        {
                            _context.Formations.Add(formation);
                            await _context.SaveChangesAsync();
                        }

                        return CreatedAtAction(
                            nameof(GetFormation),
                            new { id = formation.FormationId },
                            new
                            {
                                FormationId = formation.FormationId,
                                MoodleCourseId = moodleCourseId,
                                Message = "Formation created successfully"
                            });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating formation");
                        return StatusCode(500, new
                        {
                            Error = "Failed to create formation",
                            Details = ex.Message
                        });
                    }
                }

                [HttpGet("{id}")]
                public async Task<IActionResult> GetFormation(int id)
                {
                    try
                    {
                        var formation = await _context.Formations
                            .Include(f => f.Courses)
                                .ThenInclude(c => c.Test)
                                    .ThenInclude(t => t.Questions)
                            .FirstOrDefaultAsync(f => f.FormationId == id);

                        if (formation == null)
                        {
                            return NotFound();
                        }

                        return Ok(formation);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting formation with ID {id}");
                        return StatusCode(500, "Internal server error");
                    }
                }

                [HttpPut("{id}")]
                public async Task<IActionResult> UpdateFormation(int id, [FromBody] Formation updatedFormation)
                {
                    try
                    {
                        if (id != updatedFormation.FormationId)
                        {
                            return BadRequest("ID mismatch");
                        }

                        var existingFormation = await _context.Formations
                            .Include(f => f.Courses)
                            .FirstOrDefaultAsync(f => f.FormationId == id);

                        if (existingFormation == null)
                        {
                            return NotFound();
                        }

                        existingFormation.Fullname = updatedFormation.Fullname;
                        existingFormation.Shortname = updatedFormation.Shortname;
                        existingFormation.Summary = updatedFormation.Summary;
                        existingFormation.MoodleCategoryId = updatedFormation.MoodleCategoryId;
                        existingFormation.UpdatedAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();

                        return NoContent();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating formation with ID {id}");
                        return StatusCode(500, "Internal server error");
                    }
                }*/
        [HttpDelete("{formationId}")]
        public async Task<IActionResult> DeleteFormation(int formationId)
        {
            try
            {
                await _formationService.DeleteFormationAndMoodleCourseAsync(formationId);
                return Ok($"Formation with ID {formationId}, its inscriptions, courses, and Moodle course deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting formation with ID {formationId}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }

}