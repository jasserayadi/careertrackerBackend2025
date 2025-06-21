using Career_Tracker_Backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services
{
    public interface IFeedbackService
    {
        Task<Feedback> CreateFeedbackAsync(FeedbackDto feedbackDto);
        Task<List<FeedbackDto>> GetFeedbackForFormationAsync(int formationId); // Match controller
        Task<FeedbackDto> GetFeedbackByIdAsync(int id); // Match controller
    }

    public class FeedbackDto
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public int FormationId { get; set; }
        public int Rate { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } // Derived, not in model
        public UsersDto User { get; set; }
    }

    public class UsersDto
    {
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }

    // Keep FeedbackResponseDto for reference, but unused here
    public class FeedbackResponseDto
    {
        public int Id { get; set; }
        public int Rate { get; set; }
        public string Message { get; set; }
        public int FormationId { get; set; }
        public string FormationName { get; set; }
        public string UserName { get; set; }
    }
}