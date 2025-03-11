using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Career_Tracker_Backend.Models
{
    public class Certificat
    {
        [Key]
        public int CertificatId { get; set; }
        [Required]
        [MaxLength(200)]
        public string CertificatName { get; set; }


        [ForeignKey("UserFk")]
        public virtual User User { get; set; } // Navigation property
        public int UserId { get; set; }
    }
}
