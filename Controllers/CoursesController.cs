using Career_Tracker_Backend.Services.CourseService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
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
    }
}