using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
        [Column("required_skills_json")]
        public string? RequiredSkillsJson { get; set; }

        [NotMapped]
        public List<string> RequiredSkills
        {
            get
            {
                try
                {
                    return string.IsNullOrEmpty(RequiredSkillsJson)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(RequiredSkillsJson) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            set => RequiredSkillsJson = JsonSerializer.Serialize(value ?? new List<string>());
        }
    }
}