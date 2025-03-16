using Career_Tracker_Backend.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

public class Question
{
    [Key]
    public int QuestionId { get; set; }

    [MaxLength(2000)] // Match MySQL varchar(2000)
    public string? Text { get; set; } // Remove [Required] to allow NULL

    public float Rate { get; set; } // Keep as float (ensure MySQL column is NOT NULL)

    [Required]
    [MaxLength(10)]
    public string QuestionNumber { get; set; }

    public string? HtmlContent { get; set; } // Match MySQL text (nullable)

    [Required]
    [MaxLength(50)]
    public string QuestionType { get; set; }

    public int TestFk { get; set; } // Match MySQL int (NOT NULL)

    [ForeignKey("TestFk")]
    public Test Test { get; set; }

    public string? QuestionText { get; set; } // Match MySQL text (nullable)

    public string? ChoicesJson { get; set; } // Match MySQL text (nullable)

    public string? CorrectAnswer { get; set; } // Match MySQL text (nullable)

    [NotMapped] // Not mapped to the database
    public List<string> Choices
    {
        get => string.IsNullOrEmpty(ChoicesJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(ChoicesJson);
        set => ChoicesJson = JsonSerializer.Serialize(value);
    }
}