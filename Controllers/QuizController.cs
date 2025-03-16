using Career_Tracker_Backend.Services.QuizService;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IMoodleService _moodleService;
    private readonly ILogger<QuizController> _logger;
    private readonly IQuizService _quizService;

    // Constructor for dependency injection
    public QuizController(IMoodleService moodleService, ILogger<QuizController> logger, IQuizService quizService)
    {
        _moodleService = moodleService;
        _logger = logger;
        _quizService = quizService;
    }

    /// <summary>
    /// Fetches quizzes for a specific course from Moodle and returns them as a list.
    /// </summary>
    /// <param name="courseId">The ID of the course to fetch quizzes for.</param>
    /// <returns>A list of quizzes.</returns>
    [HttpGet("quizzes/{courseId}")]
    public async Task<IActionResult> GetQuizzesByCourse(int courseId)
    {
        try
        {
            _logger.LogInformation($"Fetching quizzes for course ID: {courseId}");

            // Call the MoodleService to get quizzes
            var quizzes = await _moodleService.GetQuizzesByCourseAsync(courseId);

            if (quizzes == null || quizzes.Count == 0)
            {
                _logger.LogWarning($"No quizzes found for course ID: {courseId}");
                return NotFound($"No quizzes found for course ID: {courseId}");
            }

            _logger.LogInformation($"Successfully fetched {quizzes.Count} quizzes for course ID: {courseId}");
            return Ok(quizzes); // Return the list of quizzes
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching quizzes for course ID {courseId}: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Fetches quizzes for a specific course and saves them to the database.
    /// </summary>
    /// <param name="courseId">The ID of the course to fetch quizzes for.</param>
    /// <param name="userId">The ID of the user saving the quizzes.</param>
    /// <returns>A success or error message.</returns>
    [HttpPost("save-quizzes/{courseId}/{userId}")]
    public async Task<IActionResult> SaveQuizzesToDatabase(int courseId, int userId)
    {
        try
        {
            _logger.LogInformation($"Saving quizzes for course ID: {courseId} and user ID: {userId}");

            // Call the MoodleService to save quizzes to the database
            await _moodleService.SaveQuizDataAsync(courseId, userId);

            _logger.LogInformation($"Successfully saved quizzes for course ID: {courseId} and user ID: {userId}");
            return Ok("Quizzes saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving quizzes for course ID {courseId}: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("test/{testId}/questions")]
    public async Task<IActionResult> GetTestQuestions(int testId)
    {
        var questions = await _quizService.GetTestQuestionsAsync(testId);
        return Ok(questions);
    }
}