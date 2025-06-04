using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;
using System.Text.Json;
using System.Net;

namespace Career_Tracker_Backend.Services.UserServices
{
    public class MoodleService : IMoodleService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoodleService> _logger;
        private readonly IConfiguration _configuration;

        public MoodleService(HttpClient httpClient, ApplicationDbContext context, ILogger<MoodleService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            _logger.LogInformation($"Moodle URL from configuration: {moodleUrl}");
            if (string.IsNullOrWhiteSpace(moodleUrl) || !Uri.TryCreate(moodleUrl, UriKind.Absolute, out var baseUri))
            {
                throw new Exception("Invalid or missing Moodle URL in configuration");
            }
            _httpClient.BaseAddress = baseUri;

        }

        public async Task<bool> CreateMoodleUserAsync(string username, string firstname, string lastname, string password, string email)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("wstoken", moodleToken),
                new KeyValuePair<string, string>("wsfunction", "core_user_create_users"),
                new KeyValuePair<string, string>("moodlewsrestformat", "json"),
                new KeyValuePair<string, string>("users[0][username]", username),
                new KeyValuePair<string, string>("users[0][password]", password),
                new KeyValuePair<string, string>("users[0][firstname]", firstname),
                new KeyValuePair<string, string>("users[0][lastname]", lastname),
                new KeyValuePair<string, string>("users[0][email]", email),
            });

            var response = await _httpClient.PostAsync(moodleUrl, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMoodleUserAsync(List<int> userIds)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("wstoken", moodleToken),
                new KeyValuePair<string, string>("wsfunction", "core_user_delete_users"),
                new KeyValuePair<string, string>("moodlewsrestformat", "json")
            };

            for (int i = 0; i < userIds.Count; i++)
            {
                formData.Add(new KeyValuePair<string, string>($"userids[{i}]", userIds[i].ToString()));
            }

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new Exception($"Failed to delete user in Moodle. Response: {responseContent}");
            }
        }

        public async Task<List<MoodleCourse>> GetCoursesAsync()
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("wstoken", moodleToken),
                new KeyValuePair<string, string>("wsfunction", "core_course_get_courses"),
                new KeyValuePair<string, string>("moodlewsrestformat", "json")
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var courses = JsonConvert.DeserializeObject<List<MoodleCourse>>(responseContent);
                return courses;
            }
            else
            {
                throw new Exception($"Failed to retrieve courses from Moodle. Response: {responseContent}");
            }
        }

        public async Task<List<MoodleCourseContent>> GetCourseContentsAsync(int courseId)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "core_course_get_contents"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("courseid", courseId.ToString())
    };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var courseContents = JsonConvert.DeserializeObject<List<MoodleCourseContent>>(responseContent, settings);
                    return courseContents ?? new List<MoodleCourseContent>();
                }
                catch (System.Text.Json.JsonException ex)
                {
                    // Log the actual response for debugging
                    Console.WriteLine($"Failed to deserialize Moodle response: {responseContent}");
                    throw new Exception($"Failed to deserialize Moodle response. See logs for details. Error: {ex.Message}");
                }
            }
            else
            {
                throw new Exception($"Failed to retrieve course contents from Moodle. Status: {response.StatusCode}. Response: {responseContent}");
            }
        }

        public async Task<List<MoodleQuiz>> GetQuizzesByCourseAsync(int courseId)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8"; // Replace with your Moodle token

            var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("wstoken", moodleToken),
            new KeyValuePair<string, string>("wsfunction", "mod_quiz_get_quizzes_by_courses"),
            new KeyValuePair<string, string>("moodlewsrestformat", "json"),
            new KeyValuePair<string, string>("courseids[0]", courseId.ToString())
        };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var quizzesResponse = JsonConvert.DeserializeObject<MoodleQuizResponse>(responseContent);
                return quizzesResponse?.Quizzes ?? new List<MoodleQuiz>();
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<MoodleErrorResponse>(responseContent);
                if (errorResponse?.ErrorCode == "invalidtoken")
                {
                    _logger.LogError("Invalid token. Please check the token configuration in Moodle.");
                    throw new Exception("Invalid token. Please check the token configuration in Moodle.");
                }

                _logger.LogError($"Failed to retrieve quizzes for course ID {courseId}. Response: {responseContent}");
                throw new Exception($"Failed to retrieve quizzes for course ID {courseId}. Response: {responseContent}");
            }
        }

        public async Task SaveQuizDataAsync(int courseId, int userId)
        {
            try
            {
                _logger.LogInformation($"Fetching quizzes for course ID: {courseId}");
                var quizzes = await GetQuizzesByCourseAsync(courseId);

                if (quizzes == null || quizzes.Count == 0)
                {
                    _logger.LogWarning($"No quizzes found for course ID {courseId}.");
                    return;
                }

                _logger.LogInformation($"Found {quizzes.Count} quizzes for course ID: {courseId}");

                foreach (var quiz in quizzes)
                {
                    _logger.LogInformation($"Fetching attempts for quiz ID: {quiz.Id}, user ID: {userId}");
                    var attempts = await GetUserAttemptsAsync(quiz.Id, userId);

                    if (attempts == null || attempts.Count == 0)
                    {
                        _logger.LogWarning($"No attempts found for quiz ID {quiz.Id}, user ID {userId}.");
                        continue;
                    }

                    _logger.LogInformation($"Found {attempts.Count} attempts for quiz ID: {quiz.Id}");

                    var mostRecentAttempt = attempts.OrderByDescending(a => a.AttemptId).FirstOrDefault();

                    if (mostRecentAttempt == null)
                    {
                        _logger.LogWarning($"No recent attempt found for quiz ID {quiz.Id}, user ID {userId}.");
                        continue;
                    }

                    _logger.LogInformation($"Fetching questions for quiz ID: {quiz.Id}, attempt ID: {mostRecentAttempt.AttemptId}");
                    var questions = await GetQuizQuestionsAsync(quiz.Id, mostRecentAttempt.AttemptId);

                    if (questions == null || questions.Count == 0)
                    {
                        _logger.LogWarning($"No questions found for quiz ID {quiz.Id}, attempt ID {mostRecentAttempt.AttemptId}.");
                        continue;
                    }

                    _logger.LogInformation($"Found {questions.Count} questions for quiz ID: {quiz.Id}");

                    // Check if the test already exists in the database
                    var existingTest = await _context.Tests
                        .Include(t => t.Questions) // Include related questions
                        .FirstOrDefaultAsync(t => t.MoodleQuizId == quiz.Id && t.CourseId == courseId);

                    if (existingTest == null)
                    {
                        // Create a new Test entity if it doesn't exist
                        var test = new Test
                        {
                            MoodleQuizId = quiz.Id,
                            Title = quiz.Name,
                            Description = quiz.Intro,
                            CourseId = courseId,
                            Questions = questions.Select(q => new Question
                            {
                                Text = q.Html.Length > 2000 ? q.Html.Substring(0, 2000) : q.Html, // Truncate if necessary
                                QuestionType = q.Type,
                                Rate = q.Answers?.FirstOrDefault()?.Fraction ?? 0,
                                QuestionNumber = q.Id.ToString(),
                                HtmlContent = q.Html,
                                QuestionText = q.QuestionText, // Map extracted QuestionText
                                CorrectAnswer = q.CorrectAnswer, // Map extracted CorrectAnswer
                                Choices = q.Choices // Assign the Choices property
                            }).ToList()
                        };

                        // Add the Test to the database
                        _context.Tests.Add(test);
                    }
                    else
                    {
                        // Update the existing test and its questions
                        existingTest.Title = quiz.Name;
                        existingTest.Description = quiz.Intro;


                        foreach (var question in questions)
                        {
                            var existingQuestion = existingTest.Questions
                                .FirstOrDefault(q => q.QuestionNumber == question.Id.ToString());

                            if (existingQuestion == null)
                            {
                                // Add new question if it doesn't exist
                                existingTest.Questions.Add(new Question
                                {
                                    Text = question.Html.Length > 2000 ? question.Html.Substring(0, 2000) : question.Html,
                                    QuestionType = question.Type,
                                    Rate = question.Answers?.FirstOrDefault()?.Fraction ?? 0,
                                    QuestionNumber = question.Id.ToString(),
                                    HtmlContent = question.Html,
                                    QuestionText = question.QuestionText,
                                    CorrectAnswer = question.CorrectAnswer,
                                    Choices = question.Choices
                                });
                            }
                            else
                            {
                                // Update existing question
                                existingQuestion.Text = question.Html.Length > 2000 ? question.Html.Substring(0, 2000) : question.Html;
                                existingQuestion.QuestionType = question.Type;
                                existingQuestion.Rate = question.Answers?.FirstOrDefault()?.Fraction ?? 0;
                                existingQuestion.HtmlContent = question.Html;
                                existingQuestion.QuestionText = question.QuestionText;
                                existingQuestion.CorrectAnswer = question.CorrectAnswer;
                                existingQuestion.Choices = question.Choices;
                            }
                        }
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
                _logger.LogInformation("Quiz data saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving quiz data: {ex.Message}");
                throw;
            }
        }

        private async Task<List<MoodleQuizAttempt>> GetUserAttemptsAsync(int quizId, int userId)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8"; // Replace with your Moodle token

            var formData = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "mod_quiz_get_user_attempts"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("quizid", quizId.ToString()),
        new KeyValuePair<string, string>("userid", userId.ToString())
    };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Log the raw response
            _logger.LogInformation($"Raw API response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // Deserialize into the wrapper class
                var attemptsResponse = JsonConvert.DeserializeObject<MoodleQuizAttemptResponse>(responseContent);

                if (attemptsResponse == null || attemptsResponse.Attempts == null || attemptsResponse.Attempts.Count == 0)
                {
                    _logger.LogWarning($"No attempts found for quiz ID {quizId}, user ID {userId}.");
                    return new List<MoodleQuizAttempt>();
                }

                // Return the list of attempts
                return attemptsResponse.Attempts;
            }
            else
            {
                _logger.LogError($"Failed to retrieve attempts for quiz ID {quizId}, user ID {userId}. Response: {responseContent}");
                throw new Exception($"Failed to retrieve attempts for quiz ID {quizId}, user ID {userId}. Response: {responseContent}");
            }
        }

        private async Task<List<MoodleQuestion>> GetQuizQuestionsAsync(int quizId, int attemptId)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8"; // Replace with your Moodle token

            var formData = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "mod_quiz_get_attempt_review"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("attemptid", attemptId.ToString())
    };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var attemptReview = JsonConvert.DeserializeObject<MoodleQuizAttemptReview>(responseContent);
                var questions = attemptReview?.Questions ?? new List<MoodleQuestion>();

                // Parse each question to extract QuestionText, CorrectAnswer, and Choices
                foreach (var question in questions)
                {
                    _logger.LogInformation($"HTML Content for Question ID {question.Id}: {question.Html}");
                    question.QuestionText = ExtractQuestionText(question.Html);
                    question.CorrectAnswer = ExtractCorrectAnswer(question.Html);
                    question.Choices = ExtractChoices(question.Html);

                    // Log the extracted choices for debugging
                    _logger.LogInformation($"Question ID: {question.Id}, Choices: {System.Text.Json.JsonSerializer.Serialize(question.Choices)}");
                }

                return questions;
            }
            else
            {
                _logger.LogError($"Failed to retrieve questions for quiz ID {quizId}, attempt ID {attemptId}. Response: {responseContent}");
                throw new Exception($"Failed to retrieve questions for quiz ID {quizId}, attempt ID {attemptId}. Response: {responseContent}");
            }
        }
        private string ExtractQuestionText(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var questionTextNode = htmlDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'qtext')]");
            return questionTextNode?.InnerText.Trim() ?? string.Empty;
        }

        private string ExtractCorrectAnswer(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var correctAnswerNode = htmlDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'rightanswer')]");
            return correctAnswerNode?.InnerText.Trim() ?? string.Empty;
        }

        private List<string> ExtractChoices(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var choices = new List<string>();

            // Update the XPath query to match the structure of the HTML
            var choiceNodes = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'answer')]//div[contains(@class, 'd-flex')]");

            if (choiceNodes != null)
            {
                _logger.LogInformation($"Found {choiceNodes.Count} choices in the HTML content.");
                foreach (var choiceNode in choiceNodes)
                {
                    // Extract the choice text from the <div class="flex-fill ms-1"> element
                    var choiceTextNode = choiceNode.SelectSingleNode(".//div[contains(@class, 'flex-fill')]");
                    if (choiceTextNode != null)
                    {
                        var choiceText = choiceTextNode.InnerText.Trim();

                        // Unescape HTML entities
                        choiceText = WebUtility.HtmlDecode(choiceText);

                        _logger.LogInformation($"Choice: {choiceText}");
                        choices.Add(choiceText);
                    }
                }
            }
            else
            {
                _logger.LogWarning("No choices found in the HTML content.");
            }

            return choices;
        }
        public QuizQuestionDetail ParseHtmlContent(string htmlContent)
        {
            var quizQuestion = new QuizQuestionDetail
            {
                Choices = new List<string>()
            };

            // Load the HTML content into an HtmlDocument
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Extract the question text
            var questionTextNode = htmlDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'qtext')]");
            if (questionTextNode != null)
            {
                quizQuestion.QuestionText = questionTextNode.InnerText.Trim();
            }

            // Extract the question type from the class attribute
            var questionType = htmlDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'que')]")?.GetAttributeValue("class", "").Split(' ')[1];
            quizQuestion.QuestionType = questionType;

            // Extract the choices (for multiple-choice questions)
            var choiceNodes = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'answer')]//div[contains(@class, 'd-flex')]");
            if (choiceNodes != null)
            {
                foreach (var choiceNode in choiceNodes)
                {
                    // Extract the choice text from the <div class="flex-fill ms-1"> element
                    var choiceTextNode = choiceNode.SelectSingleNode(".//div[contains(@class, 'flex-fill')]");
                    if (choiceTextNode != null)
                    {
                        var choiceText = choiceTextNode.InnerText.Trim();

                        // Unescape HTML entities
                        choiceText = WebUtility.HtmlDecode(choiceText);

                        quizQuestion.Choices.Add(choiceText);
                    }
                }
            }

            // Extract the correct answer (from the correct answer node)
            var correctAnswerNode = htmlDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'rightanswer')]");
            if (correctAnswerNode != null)
            {
                quizQuestion.CorrectAnswer = correctAnswerNode.InnerText.Trim();
            }

            return quizQuestion;
        }

        public class QuizQuestionDetail
        {
            public string QuestionText { get; set; } // The question text
            public string QuestionType { get; set; } // Type of question (e.g., multichoice, truefalse, shortanswer)
            public List<string> Choices { get; set; } // List of choices (for multiple-choice questions)
            public string CorrectAnswer { get; set; } // The correct answer (for multiple-choice, true/false, short answer)
        }
        public class QuizQuestion
        {
            public string QuestionText { get; set; }
            public List<string> Choices { get; set; }
            public string CorrectAnswer { get; set; }
            public string QuestionType { get; set; }
        }
        public class MoodleQuizAttempt
        {
            [JsonProperty("id")]
            public int AttemptId { get; set; }

            [JsonProperty("quiz")]
            public int QuizId { get; set; }

            [JsonProperty("userid")]
            public int UserId { get; set; }

            [JsonProperty("attempt")]
            public int AttemptNumber { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("timestart")]
            public long TimeStart { get; set; }

            [JsonProperty("timefinish")]
            public long TimeFinish { get; set; }
        }
        public class MoodleQuizAttemptResponse
        {
            [JsonProperty("attempts")]
            public List<MoodleQuizAttempt> Attempts { get; set; }
        }

        public class MoodleQuizAttemptReview
        {
            public List<MoodleQuestion> Questions { get; set; }
        }

        public class MoodleAnswer
        {
            public float Fraction { get; set; }
        }
        public class MoodleQuizResponse
        {
            [JsonProperty("quizzes")]
            public List<MoodleQuiz> Quizzes { get; set; }
        }
        public class MoodleQuiz
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("course")]
            public int CourseId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("intro")]
            public string Intro { get; set; }

            [JsonProperty("timeopen")]
            public int TimeOpen { get; set; }

            [JsonProperty("timeclose")]
            public int TimeClose { get; set; }

            [JsonProperty("timelimit")]
            public int TimeLimit { get; set; }

            [JsonProperty("preferredbehaviour")]
            public string PreferredBehaviour { get; set; }

            [JsonProperty("attempts")]
            public int Attempts { get; set; }
        }
        // Define classes for deserializing Moodle responses
        public class MoodleDeleteUserResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("warnings")]
            public List<string> Warnings { get; set; }
        }
        public class MoodleErrorResponse
        {
            [JsonProperty("exception")]
            public string Exception { get; set; }

            [JsonProperty("errorcode")]
            public string ErrorCode { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
        public class MoodleCourse
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("shortname")]
            public string Shortname { get; set; }

            [JsonProperty("fullname")]
            public string Fullname { get; set; }

            [JsonProperty("summary")]
            public string Summary { get; set; }

            [JsonProperty("categoryid")]
            public int Categoryid { get; set; }
        }

        public class MoodleCourseContent
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Visible { get; set; }
            public string Summary { get; set; }
            public int Summaryformat { get; set; }
            public int Section { get; set; }
            public int Hiddenbynumsections { get; set; }
            public bool Uservisible { get; set; }
            public List<MoodleModule> Modules { get; set; }
        }

        public class MoodleContent
        {
            public string Type { get; set; }
            public string Filename { get; set; }
            public string Filepath { get; set; }
            public int Filesize { get; set; }
            public string Fileurl { get; set; }
            public string Content { get; set; }
            public long Timecreated { get; set; }
            public long Timemodified { get; set; }
            public int Sortorder { get; set; }
            public int? Userid { get; set; }
            public string Author { get; set; }
            public string License { get; set; }
        }

        public class MoodleModule
        {
            public int Id { get; set; }
            public string Url { get; set; }
            public string Name { get; set; }
            public int Instance { get; set; }
            public int Contextid { get; set; }
            public int Visible { get; set; }
            public bool Uservisible { get; set; }
            public int Visibleoncoursepage { get; set; }
            public string ModIcon { get; set; }
            public string ModName { get; set; }
            public string Purpose { get; set; }
            public bool Branded { get; set; }
            public string Modplural { get; set; }
            public string Availability { get; set; }
            public int Indent { get; set; }
            public string Onclick { get; set; }
            public string Afterlink { get; set; }

            // Change these to dynamic or JToken to handle both objects and arrays
            [JsonProperty("activitybadge")]
            public dynamic ActivityBadge { get; set; }

            public string Customdata { get; set; }
            public bool Noviewlink { get; set; }
            public int Completion { get; set; }
            public int Downloadcontent { get; set; }

            [JsonProperty("dates")]
            public dynamic Dates { get; set; }

            public int Groupmode { get; set; }
            public List<MoodleContent> Contents { get; set; }
        }
        public class MoodleQuestion
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public string Html { get; set; }
            public List<MoodleAnswer> Answers { get; set; }

            // Add these properties
            public string QuestionText { get; set; }
            public string CorrectAnswer { get; set; }
            public List<string> Choices { get; set; }
        }
        public async Task<List<MoodleEnrolledUser>> GetEnrolledUsersAsync(int courseId)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "core_enrol_get_enrolled_users"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("courseid", courseId.ToString())
    };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var enrolledUsers = JsonConvert.DeserializeObject<List<MoodleEnrolledUser>>(responseContent);
                return enrolledUsers;
            }
            else
            {
                throw new Exception($"Failed to retrieve enrolled users from Moodle. Response: {responseContent}");
            }
        }
        public class MoodleEnrolledUser
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("firstname")]
            public string Firstname { get; set; }

            [JsonProperty("lastname")]
            public string Lastname { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("enrolledsince")]
            public long EnrolledSince { get; set; } // Unix timestamp
        }
        public async Task<List<MoodleUser>> GetUsersByFieldAsync(string field, List<string> values)
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentException("Field cannot be null or empty.");
            }

            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("Values cannot be null or empty.");
            }

            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("wstoken", moodleToken),
            new KeyValuePair<string, string>("wsfunction", "core_user_get_users_by_field"),
            new KeyValuePair<string, string>("field", field),
            new KeyValuePair<string, string>("moodlewsrestformat", "json")
        };

            // Add values to the form data
            for (int i = 0; i < values.Count; i++)
            {
                formData.Add(new KeyValuePair<string, string>($"values[{i}]", values[i]));
            }

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Check if the response is an error object
                if (responseContent.Contains("\"exception\""))
                {
                    // Handle Moodle API error
                    var errorResponse = JsonConvert.DeserializeObject<MoodleErrorResponse>(responseContent);
                    throw new Exception($"Moodle API Error: {errorResponse.Message} (Code: {errorResponse.ErrorCode})");
                }

                // Deserialize the response as a list of MoodleUser objects
                var moodleUsers = JsonConvert.DeserializeObject<List<MoodleUser>>(responseContent);
                return moodleUsers;
            }
            else
            {
                throw new Exception($"Failed to retrieve users from Moodle. Response: {responseContent}");
            }
        }
        public class MoodleUser
        {
            [JsonProperty("id")]
            public int Id { get; set; } // Moodle's user ID

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("firstname")]
            public string FirstName { get; set; }

            [JsonProperty("lastname")]
            public string LastName { get; set; }
        }

        public class MoodleEnrolledCourse
        {
            [JsonProperty("id")]
            public int Id { get; set; } // Moodle course ID

            [JsonProperty("fullname")]
            public string FullName { get; set; }

            [JsonProperty("shortname")]
            public string ShortName { get; set; }

            [JsonProperty("timecreated")]
            public long TimeCreated { get; set; } // Enrollment timestamp
        }





        public async Task<int?> GetMoodleUserIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.MoodleUserId;
        }
        public async Task<MoodleCompletionStatus> GetCourseCompletionStatusAsync(int userId, int courseId)
        {
            var moodleUserId = await GetMoodleUserIdAsync(userId);
            if (moodleUserId == null)
            {
                throw new Exception("Moodle user ID not found for the given user.");
            }

            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var content = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "core_completion_get_course_completion_status"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("userid", moodleUserId.ToString()), // Use moodleUserId here
        new KeyValuePair<string, string>("courseid", courseId.ToString()),
    });

            var response = await _httpClient.PostAsync(moodleUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var completionStatus = JsonConvert.DeserializeObject<MoodleCompletionStatus>(responseContent);

                if (completionStatus?.CompletionStatus == null)
                {
                    throw new Exception("Completion status data is null or invalid.");
                }

                int totalActivities = completionStatus.CompletionStatus.Completions?.Count ?? 0;
                int completedActivities = completionStatus.CompletionStatus.Completions?.Count(a => a.Complete) ?? 0;

                double percentageCompletion = totalActivities > 0 ? (double)completedActivities / totalActivities * 100 : 0;
                completionStatus.CompletionStatus.PercentageCompletion = percentageCompletion;

                return completionStatus;
            }
            else
            {
                throw new Exception($"Failed to fetch course completion status. Response: {response.StatusCode}");
            }
        }
        public class MoodleCompletionStatus
        {
            public CompletionStatus CompletionStatus { get; set; }
            public List<Warning> Warnings { get; set; }
        }

        public class CompletionStatus
        {
            public bool Completed { get; set; }
            public int Aggregation { get; set; }
            public List<Completion> Completions { get; set; }
            public double PercentageCompletion { get; set; }
        }

        public class Completion
        {
            public int Type { get; set; }
            public string Title { get; set; }
            public string Status { get; set; }
            public bool Complete { get; set; }
            public long? TimeCompleted { get; set; }
            public Details Details { get; set; }
        }

        public class Details
        {
            public string Type { get; set; }
            public string Criteria { get; set; }
            public string Requirement { get; set; }
            public string Status { get; set; }
        }

        public class Warning
        {
            public string Message { get; set; }
        }
        public async Task<List<MoodleGradeItem>> GetUserGradesAsync(int courseId, int localUserId)
        {
            // 1. Get Moodle User ID from local database
            var moodleUserId = await GetMoodleUserIdAsync(localUserId);
            if (!moodleUserId.HasValue)
            {
                throw new Exception($"No Moodle user ID found for local user {localUserId}");
            }

            // 2. Prepare Moodle API request
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
    {
        new("wstoken", moodleToken),
        new("wsfunction", "gradereport_user_get_grade_items"),
        new("moodlewsrestformat", "json"),
        new("courseid", courseId.ToString()),
        new("userid", moodleUserId.Value.ToString())
    };

            // 3. Execute the request
            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);

            // 4. Handle response
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Moodle API error: {responseContent}");
            }

            try
            {
                var result = JsonConvert.DeserializeObject<MoodleGradeReport>(responseContent);
                return result?.UserGrades?.FirstOrDefault()?.GradeItems ?? new List<MoodleGradeItem>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse Moodle response: {ex.Message}");
            }
        }

        // Your existing method (keep this)

        public class MoodleGradeReport
        {
            [JsonProperty("usergrades")]
            public List<MoodleUserGrade> UserGrades { get; set; }
        }

        public class MoodleUserGrade
        {
            [JsonProperty("gradeitems")]
            public List<MoodleGradeItem> GradeItems { get; set; }
        }

        public class MoodleGradeItem
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("itemname")]
            public string ItemName { get; set; }

            [JsonProperty("graderaw")]
            public decimal? GradeRaw { get; set; }

            [JsonProperty("gradedatesubmitted")]
            public int? DateSubmitted { get; set; }

            [JsonProperty("percentageformatted")]
            public string percentageformatted { get; set; }

        }
        public async Task<string> GetCourseNameAsync(int courseId)
        {
            try
            {
                var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
                var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

                using var httpClient = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("wstoken", moodleToken),
            new KeyValuePair<string, string>("wsfunction", "core_course_get_courses_by_field"),
            new KeyValuePair<string, string>("moodlewsrestformat", "json"),
            new KeyValuePair<string, string>("field", "id"),
            new KeyValuePair<string, string>("value", courseId.ToString())
        });

                var response = await httpClient.PostAsync(moodleUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<MoodleCourseResponse>(responseContent);
                    return result?.Courses?.FirstOrDefault()?.Fullname ?? $"Course {courseId}";
                }
            }
            catch
            {
                // Fallback if Moodle request fails
            }
            return $"Course {courseId}";
        }

        // Add these classes for Moodle response deserialization
        public class MoodleCourseResponse
        {
            public List<MoodleCourse> Courses { get; set; }
        }
        public async Task<string> GetMoodleTokenAsync(string username, string password)
        {
            try
            {
                var moodleUrl = _configuration["Moodle:BaseUrl"];
                var requestUrl = $"{moodleUrl}/login/token.php?" +
                               $"username={Uri.EscapeDataString(username)}&" +
                               $"password={Uri.EscapeDataString(password)}&" +
                               "service=moodle_mobile_app";

                var response = await _httpClient.GetAsync(requestUrl);

                // First check if we got HTML back (indicates error)
                var responseContent = await response.Content.ReadAsStringAsync();

                if (responseContent.StartsWith("<!DOCTYPE html>") ||
                    responseContent.StartsWith("<"))
                {
                    _logger.LogError("Moodle returned HTML instead of JSON. URL might be wrong.");
                    return null;
                }

                // Try parsing as JSON
                using var jsonDoc = JsonDocument.Parse(responseContent);

                if (jsonDoc.RootElement.TryGetProperty("token", out var tokenElement))
                {
                    return tokenElement.GetString();
                }

                if (jsonDoc.RootElement.TryGetProperty("error", out var errorElement))
                {
                    _logger.LogError("Moodle error: {Error}", errorElement.GetString());
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Moodle token");
                return null;
            }
        }


        private async Task<string?> GetMoodleUserId(string username)
        {
            try
            {
                var _moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
                var _moodleWSToken = "aba8b4d828c431ef68123b83f5a9cba8";
                var requestUrl = $"{_moodleUrl}/webservice/rest/server.php" +
                    $"?wstoken={_moodleWSToken}" +
                    $"&wsfunction=core_user_get_users_by_field" +
                    $"&field=username" +
                    $"&values[0]={Uri.EscapeDataString(username)}" +
                    "&moodlewsrestformat=json";

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);

                if (jsonDoc.RootElement.EnumerateArray().Any())
                {
                    return jsonDoc.RootElement[0].GetProperty("id").GetInt32().ToString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        // In MoodleService.cs
        public async Task<bool> UpdateMoodleUserAsync(int moodleUserId, string username, string firstname,
            string lastname, string email, string password = null)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var parameters = new List<KeyValuePair<string, string>>
    {
        new("wstoken", moodleToken),
        new("wsfunction", "core_user_update_users"),
        new("moodlewsrestformat", "json"),
        new("users[0][id]", moodleUserId.ToString()),
        new("users[0][username]", username),
        new("users[0][firstname]", firstname),
        new("users[0][lastname]", lastname),
        new("users[0][email]", email)
    };

            // Only add password if it's being changed
            if (!string.IsNullOrEmpty(password))
            {
                parameters.Add(new KeyValuePair<string, string>("users[0][password]", password));
            }

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(moodleUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Moodle update failed: {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            return true;
        }
        public async Task<int> CreateMoodleCourseAsync(Formation formation)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            // Calculate number of sections needed based on courses
            int numsections = formation.Courses?.Count ?? 1;

            var content = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "core_course_create_courses"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("courses[0][fullname]", formation.Fullname),
        new KeyValuePair<string, string>("courses[0][shortname]", formation.Shortname),
        new KeyValuePair<string, string>("courses[0][categoryid]", formation.MoodleCategoryId.ToString()),
        new KeyValuePair<string, string>("courses[0][summary]", formation.Summary ?? ""),
        new KeyValuePair<string, string>("courses[0][format]", "topics"),
        new KeyValuePair<string, string>("courses[0][numsections]", numsections.ToString()),
        new KeyValuePair<string, string>("courses[0][hiddensections]", "0"),
        new KeyValuePair<string, string>("courses[0][courseformatoptions][0][name]", "numsections"),
        new KeyValuePair<string, string>("courses[0][courseformatoptions][0][value]", numsections.ToString()),
    });

            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<List<MoodleCourseCreationResult>>(responseContent);
                return result?.FirstOrDefault()?.Id ?? throw new Exception("Failed to get created course ID");
            }
            else
            {
                throw new Exception($"Failed to create Moodle course. Response: {responseContent}");
            }
        }
        public class MoodleCourseCreationResult
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("shortname")]
            public string Shortname { get; set; }
        }

        public class MoodleSectionCreationResult
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
        public async Task<int> CreateMoodleQuizAsync(int moodleCourseId, Test test)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var content = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "mod_quiz_add_quiz"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("course", moodleCourseId.ToString()),
        new KeyValuePair<string, string>("name", test.Title),
        new KeyValuePair<string, string>("intro", test.Description ?? ""),
        new KeyValuePair<string, string>("timeopen", "0"),
        new KeyValuePair<string, string>("timeclose", "0"),
        new KeyValuePair<string, string>("timelimit", "0"),
        new KeyValuePair<string, string>("preferredbehaviour", "deferredfeedback"),
        new KeyValuePair<string, string>("attempts", "0"), // unlimited attempts
        new KeyValuePair<string, string>("grademethod", "1"), // highest grade
        new KeyValuePair<string, string>("decimalpoints", "2"),
        new KeyValuePair<string, string>("questiondecimalpoints", "-1"),
        new KeyValuePair<string, string>("reviewattempt", "69888"),
        new KeyValuePair<string, string>("reviewcorrectness", "4352"),
        new KeyValuePair<string, string>("reviewmarks", "4352"),
        new KeyValuePair<string, string>("reviewspecificfeedback", "4352"),
        new KeyValuePair<string, string>("reviewgeneralfeedback", "4352"),
        new KeyValuePair<string, string>("reviewrightanswer", "4352"),
        new KeyValuePair<string, string>("reviewoverallfeedback", "4352"),
        new KeyValuePair<string, string>("questionsperpage", "1"),
        new KeyValuePair<string, string>("shuffleanswers", "1"),
        new KeyValuePair<string, string>("grade", "10"), // maximum grade
    });

            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<MoodleQuizCreationResult>(responseContent);
                return result?.QuizId ?? throw new Exception("Failed to get created quiz ID");
            }
            else
            {
                throw new Exception($"Failed to create Moodle quiz. Response: {responseContent}");
            }
        }

        public class MoodleQuizCreationResult
        {
            [JsonProperty("quizid")]
            public int QuizId { get; set; }
        }
        public async Task<List<MoodleBook>> GetMoodleBooksForCourse(int courseId)
        {
            var content = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("wstoken", _configuration["Moodle:Token"]),
        new KeyValuePair<string, string>("wsfunction", "mod_book_get_books_by_courses"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("courseids[0]", courseId.ToString()),
    });

            var response = await _httpClient.PostAsync("", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get book info: {responseContent}");
            }

            var booksResponse = JsonConvert.DeserializeObject<MoodleBooksResponse>(responseContent);
            return booksResponse?.Books ?? new List<MoodleBook>();
        }
        public async Task<string> GetBookContentAsync(int courseId, int? bookId = null)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("wstoken", _configuration["Moodle:Token"]),
            new KeyValuePair<string, string>("wsfunction", "mod_book_get_books_by_courses"),
            new KeyValuePair<string, string>("moodlewsrestformat", "json"),
            new KeyValuePair<string, string>("courseids[0]", courseId.ToString()),
        });

                var response = await _httpClient.PostAsync("", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get book info: {responseContent}");
                }

                var booksResponse = JsonConvert.DeserializeObject<MoodleBooksResponse>(responseContent);
                var book = bookId.HasValue
                    ? booksResponse?.Books?.FirstOrDefault(b => b.Id == bookId.Value)
                    : booksResponse?.Books?.FirstOrDefault(); // Select first book if bookId is not provided

                if (book == null)
                {
                    throw new Exception("Book not found in course");
                }

                // Get book chapters
                var chaptersContent = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("wstoken", _configuration["Moodle:Token"]),
            new KeyValuePair<string, string>("wsfunction", "mod_book_get_book_chapters"),
            new KeyValuePair<string, string>("moodlewsrestformat", "json"),
            new KeyValuePair<string, string>("bookid", book.Id.ToString()),
        });

                var chaptersResponse = await _httpClient.PostAsync("", chaptersContent);
                var chaptersResponseContent = await chaptersResponse.Content.ReadAsStringAsync();

                if (!chaptersResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to get book chapters: {chaptersResponseContent}");
                }

                var chapters = JsonConvert.DeserializeObject<MoodleBookChaptersResponse>(chaptersResponseContent);

                // Format the content for display
                var formattedContent = FormatBookContent(book, chapters?.Chapters);

                return formattedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting book content");
                throw;
            }
        }

        private string FormatBookContent(MoodleBook book, List<MoodleBookChapter> chapters)
        {
            if (chapters == null || !chapters.Any())
            {
                return book.Intro ?? "No content available for this book.";
            }

            var htmlContent = new System.Text.StringBuilder();
            htmlContent.Append($"<h1>{book.Name}</h1>");

            if (!string.IsNullOrEmpty(book.Intro))
            {
                htmlContent.Append($"<div class='book-intro'>{book.Intro}</div>");
            }

            htmlContent.Append("<ul class='book-chapters'>");
            foreach (var chapter in chapters.OrderBy(c => c.ChapterNumber))
            {
                htmlContent.Append($"<li><h2>{chapter.Title}</h2>");
                htmlContent.Append($"<div class='chapter-content'>{chapter.Content}</div></li>");
            }
            htmlContent.Append("</ul>");

            return htmlContent.ToString();
        }
    }

    // Add these DTO classes to your project
    public class MoodleBooksResponse
    {
        [JsonProperty("books")]
        public List<MoodleBook> Books { get; set; }
    }

    public class MoodleBook
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("introformat")]
        public int IntroFormat { get; set; }
    }

    public class MoodleBookChaptersResponse
    {
        [JsonProperty("chapters")]
        public List<MoodleBookChapter> Chapters { get; set; }
    }

    public class MoodleBookChapter
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("chapter")]
        public int ChapterNumber { get; set; }
    
}




}