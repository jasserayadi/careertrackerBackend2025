using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Tracker_Backend.Models
{
    public class Certificat
    {
        [Key]
        public int CertificatId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CertificatName { get; set; }

        public string CertificateNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpirationDate { get; set; } = DateTime.UtcNow.AddYears(2); // Default 2-year expiry

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int CourseId { get; set; }
        public string VerificationCode { get; set; } = Guid.NewGuid().ToString("N");
        public string? PdfUrl { get; set; } // Stores PDF download link
    }
}