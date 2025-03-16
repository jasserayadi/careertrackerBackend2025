using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;

namespace Career_Tracker_Backend.Services.UserServices
{
    public class MoodleService : IMoodleService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoodleService> _logger;

        public MoodleService(HttpClient httpClient, ApplicationDbContext context, ILogger<MoodleService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
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
                var courseContents = JsonConvert.DeserializeObject<List<MoodleCourseContent>>(responseContent);
                return courseContents;
            }
            else
            {
                throw new Exception($"Failed to retrieve course contents from Moodle. Response: {responseContent}");
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
                            HtmlContent = q.Html
                        }).ToList()
                    };

                    _context.Tests.Add(test);
                }

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
                return attemptReview?.Questions ?? new List<MoodleQuestion>();
            }
            else
            {
                _logger.LogError($"Failed to retrieve questions for quiz ID {quizId}, attempt ID {attemptId}. Response: {responseContent}");
                throw new Exception($"Failed to retrieve questions for quiz ID {quizId}, attempt ID {attemptId}. Response: {responseContent}");
            }
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
            var choiceNodes = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'answer')]//label");
            if (choiceNodes != null)
            {
                foreach (var choiceNode in choiceNodes)
                {
                    quizQuestion.Choices.Add(choiceNode.InnerText.Trim());
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
        public class MoodleQuestion
        {
            public int Id { get; set; }
            public string Html { get; set; }
            public string Type { get; set; }
            public List<MoodleAnswer> Answers { get; set; }
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
            public List<object> Activitybadge { get; set; }
            public string Customdata { get; set; }
            public bool Noviewlink { get; set; }
            public int Completion { get; set; }
            public int Downloadcontent { get; set; }
            public List<object> Dates { get; set; }
            public int Groupmode { get; set; }
            public List<MoodleContent> Contents { get; set; }
        }

    }
}