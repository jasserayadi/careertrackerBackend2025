using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Tracker_Backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string Firstname { get; set; }

        [Required]
        [MaxLength(100)]
        public string Lastname { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Column("date_creation")]
        public DateTimeOffset? DateCreation { get; set; } // Nullable

        public int? RoleId { get; set; } // Nullable
        public int? JobId { get; set; } // Nullable
        public int? BadgeId { get; set; } // Nullable
        public int? RoleIdRole { get; set; } // Nullable

        [ForeignKey("BadgeId")]
        public Badge? Badge { get; set; } // Nullable

        public Role? Role { get; set; } // Nullable
        public CV? CV { get; set; } // Nullable

        public Job? Job { get; set; } // Nullable


        public ICollection<Inscription>? Inscriptions { get; set; } = new List<Inscription>();
        public ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Certificat>? Certificats { get; set; } = new List<Certificat>();
        public ICollection<Formation>? Formations { get; set; } = new List<Formation>();

        public bool VerifyPassword(string password)
        {
            return Password == password; // Replace with hashed password verification
        }
    }
}