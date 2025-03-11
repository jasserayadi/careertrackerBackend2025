namespace Career_Tracker_Backend.Models
{
    public class Category
    {
        public int CategoryId { get; set; } // Primary key
        public string Name { get; set; }

        // Navigation property for the one-to-many relationship with Course
     //   public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}