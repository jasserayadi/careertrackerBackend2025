using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.CourseService;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ILogger<CourseService> _logger;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context;
        private readonly IMoodleService _moodleService;
        public CourseController(ICourseService courseService, ApplicationDbContext context, IMoodleService moodleService, ILogger<CourseService> logger)
        {
            _courseService = courseService;
            _context = context;
            _moodleService = moodleService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
        [HttpGet("content/{courseId}/{bookId?}")]
        public async Task<IActionResult> GetBookContent(int courseId, int? bookId = null)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.MoodleCourseId == courseId);

                if (course == null)
                {
                    return NotFound("Course not found in our system");
                }

                var content = await _moodleService.GetBookContentAsync(courseId, bookId);

                return Ok(new { content });
            }
            catch (Exception ex) when (ex.Message == "Book not found in course")
            {
                return NotFound(new { error = "Book not found in course" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting book content for course {courseId}, book {bookId}");
                return StatusCode(500, new { error = "Failed to get book content", details = ex.Message });
            }
        }



        [HttpGet("content/{courseId}")]
        public async Task<IActionResult> GetBookContent(int courseId)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.MoodleCourseId == courseId);

                if (course == null)
                {
                    return NotFound("Course not found in our system");
                }

                if (!course.MoodleBookId.HasValue)
                {
                    return NotFound("No book associated with this course");
                }

                var content = await _moodleService.GetBookContentAsync(courseId, course.MoodleBookId.Value);

                return Ok(new { content });
            }
            catch (Exception ex) when (ex.Message == "Book not found in course")
            {
                return NotFound(new { error = "Book not found in course" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting book content for course {courseId}");
                return StatusCode(500, new { error = "Failed to get book content", details = ex.Message });
            }
        }
        [HttpPost("sync-books/{formationId}")]
        public async Task<IActionResult> SyncBookIds(int formationId)
        {
            try
            {
                // Get all courses for this formation that don't have a MoodleBookId yet
                var courses = await _context.Courses
                    .Where(c => c.FormationId == formationId && !c.MoodleBookId.HasValue)
                    .ToListAsync();

                if (!courses.Any())
                {
                    return Ok(new { message = "All courses already have book IDs" });
                }

                int updatedCount = 0;

                foreach (var course in courses)
                {
                    if (course.MoodleCourseId.HasValue)
                    {
                        // Get books for this course from Moodle
                        var books = await _moodleService.GetMoodleBooksForCourse(course.MoodleCourseId.Value);

                        if (books.Any())
                        {
                            course.MoodleBookId = books.First().Id;
                            updatedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Successfully updated book IDs for {updatedCount} courses",
                    updatedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing book IDs");
                return StatusCode(500, new { error = "Book ID sync failed", details = ex.Message });
            }
        }
    }
}