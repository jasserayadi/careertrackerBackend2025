using Career_Tracker_Backend.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace Career_Tracker_Backend.Models
{
    public class Badge
    {
        public int BadgeId { get; set; }
        [Column(TypeName = "nvarchar(24)")]
        public BadgeName BadgeName { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }

   
}