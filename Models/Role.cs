using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Role
    {
        [Key]
        public int IdRole { get; set; }
        [Column(TypeName = "nvarchar(24)")]
        public RoleName RoleName { get; set; }
        public virtual User User { get; set; }
    }

    
}
