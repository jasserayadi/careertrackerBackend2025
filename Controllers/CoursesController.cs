using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.CourseService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ILogger<CourseService> _logger;
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost("sync/{moodleCourseId}")]
        public async Task<IActionResult> SyncCourseContents(int moodleCourseId)
        {
            await _courseService.SyncCourseContentsFromMoodleAsync(moodleCourseId);
            return Ok("Course contents synced successfully.");
        }
        [HttpGet]
        public async Task<ActionResult<List<Course>>> GetCourses()
        {
            var courses = await _courseService.GetCoursesAsync();
            return Ok(courses);
        }
        [HttpGet("by-formation/{formationId}")]
        public async Task<ActionResult<List<Course>>> GetCoursesByFormationId(int formationId)
        {
            var courses = await _courseService.GetCoursesByFormationIdAsync(formationId);
            if (courses == null || !courses.Any())
            {
                return NotFound("Aucun cours trouvé pour cette formation.");
            }
            return Ok(courses);
        }
    }
}