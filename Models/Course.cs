using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Tracker_Backend.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Summary { get; set; } // Allow NULL

        [Column(TypeName = "text")]
        public string? Content { get; set; } // Allow NULL

        public int? MoodleCourseId { get; set; } // Allow NULL
        public int? MoodleSectionId { get; set; } // Allow NULL

        [MaxLength(200)]
        public string? Url { get; set; } // Allow NULL

        public string? ModName { get; set; } // Allow NULL
        public string? ModIcon { get; set; } // Allow NULL
        public string? ModPurpose { get; set; } // Allow NULL

        [ForeignKey("FormationId")]
        public virtual Formation? Formation { get; set; } // Allow NULL
        public int FormationId { get; set; }

        public virtual Test? Test { get; set; } // Allow NULL

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow; // Allow NULL
        public DateTime? UpdatedAt { get; set; } // Allow NULL
    }
}