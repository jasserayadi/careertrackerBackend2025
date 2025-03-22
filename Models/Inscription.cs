using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Tracker_Backend.Models
{
    public class Inscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InscriptionId { get; set; }

        [Required]
        [Column("inscription_date", TypeName = "datetime")]
        public DateTime InscriptionDate { get; set; } // Maps to Moodle's `timecreated`

        // Foreign keys
        [Required]
        [Column("user_fk")]
        public int UserFk { get; set; } // Foreign key to User

        [Required]
        [Column("formation_fk")]
        public int FormationFk { get; set; } // Foreign key to Formation

        // Relationships
        [ForeignKey("UserFk")]
        public virtual User User { get; set; } // Navigation property

        [ForeignKey("FormationFk")]
        public virtual Formation Formation { get; set; } // Navigation property

        // Moodle-specific fields
        [Column("moodle_enrollment_id")]
        public int MoodleEnrollmentId { get; set; } // Stores Moodle's enrollment ID for synchronization
    }
}