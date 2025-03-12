namespace Career_Tracker_Backend.Models
{
    public class DTO
    {
        public class JobDto
        {
            public int JobId { get; set; }
            public string JobName { get; set; }
            public string JobDescription { get; set; }
            public List<UserDto> Users { get; set; } = new List<UserDto>();
        }

        public class UserDto
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public RoleName Role { get; set; }
            public IFormFile CvFile { get; set; }

        }
    }
}
