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
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public int Order { get; set; }
        public int MoodleCourseId { get; set; }
        public int MoodleSectionId { get; set; }

        // Assurez-vous que ces lignes sont bien supprimées
        // public int CategoryId { get; set; }
        // public Category Category { get; set; }
        [ForeignKey("FormationFk")]
        public virtual Formation Formation { get; set; } // Navigation property
        public int FormationId { get; set; }
        public virtual Test Test { get; set; } // Navigation property
    }

}