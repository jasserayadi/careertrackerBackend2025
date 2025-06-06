﻿using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CvController : ControllerBase
    {
        private readonly ICvService _cvService;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public CvController(ICvService cvService, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _cvService = cvService;
            _env = env;
            _context = context;
        }

        [HttpPost("process")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CV>> ProcessCv(
    [Required] IFormFile file,
    [Required] int userId)
        {
            // Supported file types
            var supportedContentTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/tiff" };

            if (!supportedContentTypes.Contains(file.ContentType))
                return BadRequest("Only PDF, JPEG, PNG, and TIFF files are supported");

            try
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsPath, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var cv = new CV
                {
                    CvFile = fileName,  // Only save fileName, not full path
                    UserId = userId
                };

                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();

                var processedCv = await _cvService.ExtractFromStoredCvAsync(cv);
                return Ok(processedCv);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing CV: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/extract")]
        public async Task<ActionResult<CV>> ExtractFromUserCv(int userId)
        {
            try
            {
                var cv = await _context.CVs.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cv == null) return NotFound();

                var processedCv = await _cvService.ExtractFromStoredCvAsync(cv);

                // Return full URL
                processedCv.CvFile = $"http://localhost:5054/uploads/{processedCv.CvFile?.TrimStart('/')}";
                return Ok(processedCv);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error extracting from CV: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CV>> GetCv(int id)
        {
            var cv = await _context.CVs.FindAsync(id);
            if (cv == null) return NotFound();

            cv.Skills = !string.IsNullOrEmpty(cv.SkillsJson)
                ? JsonSerializer.Deserialize<List<string>>(cv.SkillsJson) ?? new List<string>()
                : new List<string>();

            cv.Experiences = !string.IsNullOrEmpty(cv.ExperiencesJson)
                ? JsonSerializer.Deserialize<List<string>>(cv.ExperiencesJson) ?? new List<string>()
                : new List<string>();

            cv.CvFile = $"http://localhost:5054/uploads/{cv.CvFile}";
            return Ok(cv);
        }
        [HttpPost("user/{userId}/extract")]
        [Produces("text/plain")]
        public async Task<ActionResult<string>> ExtractFromUserCvv(int userId)
        {
            try
            {
                // Find the latest CV for the user
                var cv = await _context.CVs
                    .Where(c => c.UserId == userId)

                    .FirstOrDefaultAsync();

                if (cv == null)
                {
                    return NotFound("No CV found for this user");
                }

                // Process the CV
                var processedCv = await _cvService.ExtractFromStoredCvAsync(cv);

                // Format the response as plain text
                var responseText = new StringBuilder();
                responseText.AppendLine($"CV processed successfully for user {userId}");
                responseText.AppendLine($"CV File: {processedCv.CvFile}");
                responseText.AppendLine();

                responseText.AppendLine("Skills:");
                foreach (var skill in processedCv.Skills)
                {
                    responseText.AppendLine($"- {skill}");
                }

                responseText.AppendLine();
                responseText.AppendLine("Experiences:");
                foreach (var exp in processedCv.Experiences)
                {
                    responseText.AppendLine($"- {exp}");
                }

                return Ok(responseText.ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error extracting from CV: {ex.Message}");
            }
        }
    }

}