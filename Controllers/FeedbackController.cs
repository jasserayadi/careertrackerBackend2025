using Microsoft.AspNetCore.Mvc;
using Career_Tracker_Backend.Services;
using Career_Tracker_Backend.Models;
using System.Threading.Tasks;
using System;

namespace Career_Tracker_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDto feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var feedback = await _feedbackService.CreateFeedbackAsync(feedbackDto);
                return CreatedAtAction(nameof(GetFeedback), new { id = feedback.Id }, feedback);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("formation/{formationId}")]
        public async Task<IActionResult> GetFeedbackForFormation(int formationId)
        {
            try
            {
                var feedbacks = await _feedbackService.GetFeedbackForFormationAsync(formationId);
                return Ok(feedbacks);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedback(int id)
        {
            try
            {
                var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
                return Ok(feedback);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}