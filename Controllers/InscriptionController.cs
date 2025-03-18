using Career_Tracker_Backend.Services.InscriptionService;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class InscriptionController : ControllerBase
{
    private readonly IInscriptionService _inscriptionService;

    public InscriptionController(IInscriptionService inscriptionService)
    {
        _inscriptionService = inscriptionService;
    }

    [HttpPost("sync-enrollments/{courseId}")]
    public async Task<IActionResult> SyncEnrollments(int courseId)
    {
        try
        {
            await _inscriptionService.SyncEnrollmentsAsync(courseId);
            return Ok("Enrollments synced successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}