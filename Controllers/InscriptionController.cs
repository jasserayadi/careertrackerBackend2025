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

    /*   [HttpPost("sync")]
       public async Task<IActionResult> SyncInscriptions()
       {
           try
           {
               await _inscriptionService.SyncCourseInscriptionsAsync(; // Use _inscriptionService
               return Ok("Course inscriptions synced successfully.");
           }
           catch (Exception ex)
           {
               return StatusCode(500, $"Error syncing course inscriptions: {ex.Message}");
           }
       }*/
  
    [HttpPost("update-moodle-user-ids")]
    public async Task<IActionResult> UpdateMoodleUserIds()
    {
        try
        {
            await _inscriptionService.UpdateMoodleUserIdsAsync();
            return Ok(new { Message = "MoodleUserIds updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while updating MoodleUserIds.", Error = ex.Message });
        }
    }
    [HttpPost("SyncEnrollments/{courseId}")]
    public async Task<IActionResult> SyncEnrollments(int courseId)
    {
        try
        {
            await _inscriptionService.SyncEnrollmentsFromMoodleAsync(courseId);
            return Ok($"Enrollments for course ID {courseId} synced successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while syncing enrollments: {ex.Message}");
        }
    }
    [HttpGet("ByCourse")]
    public async Task<IActionResult> GetInscriptionsByCourse()
    {
        try
        {
            var inscriptionsByCourse = await _inscriptionService.GetInscriptionsByCourseAsync();
            return Ok(inscriptionsByCourse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while fetching inscriptions by course: {ex.Message}");
        }
    }
    [HttpGet("ByCourse/{courseId}")]
    public IActionResult GetInscriptionsByCourseId(int courseId)
    {
        var inscriptions = _inscriptionService.GetInscriptionsByCourseId(courseId);

        if (inscriptions == null || !inscriptions.Any())
        {
            return NotFound($"No inscriptions found for Course ID: {courseId}");
        }

        return Ok(inscriptions);
    }
    [HttpGet("ByUser/{userId}")]
    public IActionResult GetCoursesByUserId(int userId)
    {
        var courses = _inscriptionService.GetCoursesByUserId(userId);

        if (courses == null || !courses.Any())
        {
            return NotFound($"No courses found for User ID: {userId}");
        }

        return Ok(courses);
    }
}

