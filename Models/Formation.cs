using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Formation
    {
        [Key]
        public int FormationId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Fullname { get; set; } // Formation title
        public string Shortname { get; set; } // Formation title

        [MaxLength(10000)]
        public string Summary { get; set; } // Formation description

        // Moodle-specific fields

        public int MoodleCategoryId  { get; set; } // Maps to Moodle's course category ID


        public int MoodleCourseId { get; set; } // Maps to Moodle's parent course ID (if applicable)

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relationships
        [ForeignKey("CategoryFk")]
        public virtual Category?Category { get; set; }

          

        public virtual ICollection<Course>?Courses { get; set; } // One-to-many with Course (levels)

        public virtual ICollection<User>? Users { get; set; } // Navigation property
     
    }
}