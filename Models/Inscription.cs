using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Inscription
    {
        [Key]
        public int InscriptionId { get; set; }
        [Required]
        public DateTime InscriptionDate { get; set; } // Maps to Moodle's `timecreated`

        // Relationships
        [ForeignKey("UserFk")]
        public virtual User User { get; set; } // Navigation property

        [ForeignKey("FormationFk")]
        public virtual Formation Formation { get; set; } // Navigation property

        // Moodle-specific fields
        public int MoodleEnrollmentId { get; set; } // Stores Moodle's enrollment ID for synchronization

    }
}
