using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }
        [Required]
        [MaxLength(100)]
        public string JobName { get; set; }
        [MaxLength(1000)]
        public string JobDescription { get; set; }


        public ICollection<User> Users { get; set; } = new List<User>();
    }
}