using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Test
    {
        [Key]
        public int TestId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } // Test title
        [MaxLength(500)]
        public string Description { get; set; } // Test description

        // Moodle-specific fields
        public int MoodleQuizId { get; set; } // Maps to Moodle's quiz ID

        // Foreign key for the one-to-one relationship with Course
        public int CourseId { get; set; }

        // Navigation properties
        public Course Course { get; set; } // Navigation property
        public ICollection<Question> Questions { get; set; } = new List<Question>(); // One-to-many with Question
    }
}
