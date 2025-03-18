using Career_Tracker_Backend.Services.UserServices;
using Career_Tracker_Backend;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Career_Tracker_Backend.Services.QuizService;
using static Career_Tracker_Backend.Services.UserServices.MoodleService;
using Career_Tracker_Backend.Models;

public class QuizService: IQuizService
{
    private readonly ApplicationDbContext _context;
    private readonly IMoodleService _moodleService;

    public QuizService(ApplicationDbContext context, IMoodleService moodleService)
    {
        _context = context;
        _moodleService = moodleService;
    }

    // Method to get quiz questions for a specific test
    public async Task<List<QuizQuestionDetail>> GetTestQuestionsAsync(int testId)
    {
        // Fetch the test and its associated questions
        var test = await _context.Tests
            .Include(t => t.Questions) // Include the Questions navigation property
            .FirstOrDefaultAsync(t => t.TestId == testId);

        if (test == null)
        {
            return null; // Test not found
        }

        var quizDetails = new List<QuizQuestionDetail>();

        // Parse the HtmlContent for each question
        foreach (var question in test.Questions)
        {
            var parsedQuestions = _moodleService.ParseHtmlContent(question.HtmlContent);

            // Populate the ChoicesJson field
            question.ChoicesJson = JsonSerializer.Serialize(parsedQuestions.Choices);
            question.QuestionText = parsedQuestions.QuestionText;
            question.CorrectAnswer = parsedQuestions.CorrectAnswer;

            // Add the parsed data to the response
            quizDetails.Add(new QuizQuestionDetail
            {
                QuestionText = question.QuestionText,
                Choices = parsedQuestions.Choices,
                CorrectAnswer = question.CorrectAnswer,
                QuestionType = question.QuestionType
            });
        }

        // Save changes to the database (optional)
        await _context.SaveChangesAsync();

        return quizDetails;
    }
 
}