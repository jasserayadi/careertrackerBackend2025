using Career_Tracker_Backend.Models;
using Career_Tracker_Backend.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Career_Tracker_Backend.Services.CertificateService
{
    public class CertificateService : ICertificateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IUserService _userService;

        public CertificateService(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IUserService userService)
        {
            _context = context;
            _env = env;
            _userService = userService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<bool> IsCertificateEligible(int userId, int courseId)
        {
            var completionStatus = await _userService.GetUserCourseCompletionStatusAsync(userId, courseId);
            return completionStatus?.CompletionStatus?.PercentageCompletion >= 100;
        }

        public async Task<Certificat> GenerateAndSaveCertificate(int userId, int courseId, string courseName)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");

            var certificate = new Certificat
            {
                UserId = userId,
                CourseId = courseId,
                CertificatName = $"{courseName} Completion Certificate",
                IssueDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddYears(2),
                VerificationCode = Guid.NewGuid().ToString("N")
            };

            _context.Certificats.Add(certificate);
            await _context.SaveChangesAsync();

            var pdfBytes = await GenerateCertificatePdfContent(user, courseName, certificate);

            var uploadsPath = Path.Combine(_env.WebRootPath, "certificates");
            Directory.CreateDirectory(uploadsPath);
            var fileName = $"certificate_{certificate.CertificatId}.pdf";
            var filePath = Path.Combine(uploadsPath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            certificate.PdfUrl = $"/certificates/{fileName}";
            await _context.SaveChangesAsync();

            return certificate;
        }

        private async Task<byte[]> GenerateCertificatePdfContent(User user, string courseName, Certificat certificate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(16));

                    page.Header()
                        .AlignCenter()
                        .Text("Certificate of Completion")
                        .SemiBold().FontSize(32).FontColor(Colors.Blue.Darken3);

                    page.Content()
                        .PaddingVertical(40)
                        .Column(column =>
                        {
                            column.Spacing(20);
                            column.Item().Text($"This is to certify that {user.Firstname} {user.Lastname}")
                                .FontSize(18);
                            column.Item().Text("has successfully completed the course:")
                                .FontSize(18);
                            column.Item().Text(courseName)
                                .SemiBold().FontSize(24);
                            column.Item().Text($"Certificate ID: {certificate.CertificateNumber}");
                            column.Item().Text($"Issued on: {certificate.IssueDate:d}");
                            column.Item().Text($"Valid until: {certificate.ExpirationDate:d}");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Powered by Your Platform");
                });
            });

            return document.GeneratePdf();
        }

        public async Task<List<Certificat>> GetUserCertificates(int userId)
        {
            return await _context.Certificats
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IssueDate)
                .ToListAsync();
        }

        public async Task<Certificat?> GetCertificateById(int certificateId)
        {
            return await _context.Certificats.FindAsync(certificateId);
        }

        public async Task<Certificat?> GetCertificateForCourse(int userId, int courseId)
        {
            return await _context.Certificats
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);
        }

        public async Task<Certificat?> VerifyCertificate(string verificationCode)
        {
            return await _context.Certificats
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.VerificationCode == verificationCode);
        }
    }
}