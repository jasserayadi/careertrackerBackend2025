using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rate { get; set; }
        [Required]
        [MaxLength(1000)]
        public string message { get; set; }
        [ForeignKey("UserFk")]
        public virtual User User { get; set; } // Navigation property

    }
}
