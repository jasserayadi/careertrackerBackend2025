using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string message { get; set; }
        [ForeignKey("UserFk")]
        public virtual User User { get; set; } // Navigation property
    }
}
