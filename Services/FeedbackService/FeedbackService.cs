using Career_Tracker_Backend;
using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Career_Tracker_Backend.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> CreateFeedbackAsync(FeedbackDto feedbackDto)
        {
            var user = await _context.Users.FindAsync(feedbackDto.UserId);
            var formation = await _context.Formations.FindAsync(feedbackDto.FormationId);

            if (user == null || formation == null)
            {
                throw new ArgumentException("User or Formation not found.");
            }

            if (feedbackDto.Rate < 1 || feedbackDto.Rate > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5.");
            }

            var feedbackContent = new
            {
                FormationId = feedbackDto.FormationId,
                FormationName = formation.Fullname,
                UserFeedback = feedbackDto.Message
            };
            string message = JsonSerializer.Serialize(feedbackContent);

            if (message.Length > 1000)
            {
                throw new ArgumentException("Feedback message too long.");
            }

            var feedback = new Feedback
            {
                Rate = feedbackDto.Rate,
                message = message,
                User = user // Set navigation property
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return feedback;
        }

        public async Task<List<FeedbackDto>> GetFeedbackForFormationAsync(int formationId)
        {
            var formation = await _context.Formations.FindAsync(formationId);
            if (formation == null)
            {
                throw new ArgumentException("Formation not found.");
            }

            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .ToListAsync();

            var response = feedbacks.Select(f =>
            {
                var parsedMessage = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(f.message);
                int? parsedFormationId = parsedMessage.TryGetValue("FormationId", out var element) ? element.GetInt32() : (int?)null;
                if (parsedFormationId != formationId) return null; // Filter by FormationId from message

                return new FeedbackDto
                {
                    FeedbackId = f.Id,
                    UserId = f.User?.UserId ?? 0, // Derive from UserFk
                    FormationId = parsedFormationId ?? 0,
                    Rate = f.Rate,
                    Message = parsedMessage.TryGetValue("UserFeedback", out var feedbackElement) ? feedbackElement.GetString() : f.message,
                    CreatedAt = DateTime.UtcNow, // Fallback since not in model
                    User = new UsersDto
                    {
                        Username = f.User?.Username,
                        Firstname = f.User?.Firstname,
                        Lastname = f.User?.Lastname
                    }
                };
            }).Where(f => f != null).ToList();

            return response;
        }

        public async Task<FeedbackDto> GetFeedbackByIdAsync(int id)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
            {
                throw new ArgumentException("Feedback not found.");
            }

            var parsedMessage = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(feedback.message);
            return new FeedbackDto
            {
                FeedbackId = feedback.Id,
                UserId = feedback.User?.UserId ?? 0,
                FormationId = parsedMessage.TryGetValue("FormationId", out var formationIdElement) ? formationIdElement.GetInt32() : 0,
                Rate = feedback.Rate,
                Message = parsedMessage.TryGetValue("UserFeedback", out var feedbackElement) ? feedbackElement.GetString() : feedback.message,
                CreatedAt = DateTime.UtcNow, // Fallback since not in model
                User = new UsersDto
                {
                    Username = feedback.User?.Username,
                    Firstname = feedback.User?.Firstname,
                    Lastname = feedback.User?.Lastname
                }
            };
        }
    }
}