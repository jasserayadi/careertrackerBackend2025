using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.CertificateService;
using Career_Tracker_Backend.Services.CourseService;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Controllers
{
    [ApiController]
    [Route("api/certificates")]
    public class CertificateGenerationController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly IMoodleService _moodleService;

        public CertificateGenerationController(
            ICertificateService certificateService,
            ICourseService courseService,
            IUserService userService,
            IMoodleService moodleService)
        {
            _certificateService = certificateService;
            _courseService = courseService;
            _userService = userService;
            _moodleService = moodleService;
        }

        [HttpPost("generate/{userId}/{courseId}")]
        public async Task<IActionResult> GenerateCertificate(int userId, int courseId)
        {
            try
            {
                // Check if user exists
              

                // Get course name
                var courseName = await _moodleService.GetCourseNameAsync(courseId);
                if (string.IsNullOrEmpty(courseName))
                {
                    return NotFound(new { Message = "Course not found" });
                }

                // Check if certificate already exists
                var existingCertificate = await _certificateService.GetCertificateForCourse(userId, courseId);
                if (existingCertificate != null)
                {
                    return Conflict(new
                    {
                        Message = "Certificate already exists",
                        CertificateId = existingCertificate.CertificatId,
                        DownloadUrl = $"/api/certificates/download/{existingCertificate.CertificatId}"
                    });
                }

                // Check eligibility
                if (!await _certificateService.IsCertificateEligible(userId, courseId))
                {
                    return BadRequest(new { Message = "User is not eligible for a certificate for this course" });
                }

                // Generate and save certificate
                var certificate = await _certificateService.GenerateAndSaveCertificate(userId, courseId, courseName);

                return Ok(new
                {
                    CertificateId = certificate.CertificatId,
                    CertificateName = certificate.CertificatName,
                    IssueDate = certificate.IssueDate,
                    ExpirationDate = certificate.ExpirationDate,
                    DownloadUrl = $"/api/certificates/download/{certificate.CertificatId}",
                    VerifyUrl = $"/api/certificates/verify/{certificate.VerificationCode}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Certificate generation failed", Error = ex.Message });
            }
        }

        [HttpGet("download/{certificateId}")]
        public async Task<IActionResult> DownloadCertificate(int certificateId)
        {
            try
            {
                var certificate = await _certificateService.GetCertificateById(certificateId);
                if (certificate?.PdfUrl == null)
                {
                    return NotFound(new { Message = "Certificate not found" });
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", certificate.PdfUrl.TrimStart('/'));
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { Message = "Certificate file not found" });
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, "application/pdf", $"Certificate_{certificate.CertificatId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Certificate download failed", Error = ex.Message });
            }
        }

        [HttpGet("verify/{verificationCode}")]
        public async Task<IActionResult> VerifyCertificate(string verificationCode)
        {
            try
            {
                var certificate = await _certificateService.VerifyCertificate(verificationCode);
                if (certificate == null)
                {
                    return NotFound(new { Message = "Certificate not found or invalid verification code" });
                }

                return Ok(new
                {
                    CertificateName = certificate.CertificatName,
                    CertificateNumber = certificate.CertificateNumber,
                    IssueDate = certificate.IssueDate.ToString("yyyy-MM-dd"),
                    ExpirationDate = certificate.ExpirationDate?.ToString("yyyy-MM-dd"),
                    UserName = $"{certificate.User.Firstname} {certificate.User.Lastname}",
                    IsValid = certificate.ExpirationDate > DateTime.UtcNow,
                    DownloadUrl = $"/api/certificates/download/{certificate.CertificatId}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Certificate verification failed", Error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCertificates(int userId)
        {
            try
            {
                var certificates = await _certificateService.GetUserCertificates(userId);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve certificates", Error = ex.Message });
            }
        }
    }
}