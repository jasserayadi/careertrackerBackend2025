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
        public string Summary { get; set; }
        [Column(TypeName = "text")]
        public string Content { get; set; }

        public int? MoodleCourseId { get; set; }
        public int? MoodleSectionId { get; set; }
        [MaxLength(200)]
        public string Url { get; set; } // Module URL (e.g., "http://localhost/Mymoodle/mod/forum/view.php?id=2")
        public string ModName { get; set; } // Module type (e.g., "forum")
        public string ModIcon { get; set; } // Module icon URL (e.g., "http://localhost/Mymoodle/theme/image.php/boost/forum/1741792922/monologo?filtericon=1")
        public string ModPurpose { get; set; } // Module purpose (e.g., "collaboration")


        // Assurez-vous que ces lignes sont bien supprimées
        // public int CategoryId { get; set; }
        // public Category Category { get; set; }
        [ForeignKey("FormationId")]

        public virtual Formation? Formation { get; set; } // Navigation property
        public int FormationId { get; set; }
        public virtual Test? Test { get; set; } // Navigation property
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}