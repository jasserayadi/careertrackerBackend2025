using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Text { get; set; }
        public float Rate { get; set; }

        // Relationships
        [ForeignKey("TestFk")] // Foreign key to Test
        public virtual Test Test { get; set; } // Navigation property


    }
}
