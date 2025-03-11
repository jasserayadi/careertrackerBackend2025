using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json; // For JSON serialization

namespace Career_Tracker_Backend.Models
{
    public class CV
    {
        [Key]
        public int CvId { get; set; }

        public string? CvFile { get; set; }

        // Store Skills as a JSON string in the database
        public string? SkillsJson { get; set; }

        // Store Experiences as a JSON string in the database
        public string? ExperiencesJson { get; set; }

        // Ignore these properties in EF Core (they are not mapped to the database)
        [NotMapped]
        public List<string>? Skills
        {
            get => string.IsNullOrEmpty(SkillsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(SkillsJson);
            set => SkillsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string>? Experiences
        {
            get => string.IsNullOrEmpty(ExperiencesJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(ExperiencesJson);
            set => ExperiencesJson = JsonSerializer.Serialize(value);
        }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}