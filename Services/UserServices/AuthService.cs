using Career_Tracker_Backend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Career_Tracker_Backend.Services.UserServices
{
    public interface IMoodleAuthService
    {
        Task<MoodleAuthResult> AuthenticateWithMoodle(string username, string password);
    }

    public class AuthService : IMoodleAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.GivenName, user.Firstname ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.Lastname ?? string.Empty),
                new Claim("MoodleUserId", user.MoodleUserId?.ToString() ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public User Authenticate(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username);

            if (user == null || !user.VerifyPassword(password))
                return null;

            return user;
        }

        public async Task<MoodleAuthResult> AuthenticateWithMoodle(string username, string password)
        {
            try
            {
                var moodleConfig = _configuration.GetSection("Moodle");
                var baseUrl = moodleConfig["BaseUrl"];
                var token = moodleConfig["Token"];
                var siteUrl = moodleConfig["SiteUrl"] ?? "http://localhost/Mymoodle";

                // 1. Verify user exists in Moodle
                var userRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("wstoken", token),
                    new KeyValuePair<string, string>("wsfunction", "core_user_get_users_by_field"),
                    new KeyValuePair<string, string>("field", "username"),
                    new KeyValuePair<string, string>("values[0]", username),
                    new KeyValuePair<string, string>("moodlewsrestformat", "json")
                });

                var userResponse = await _httpClient.PostAsync(baseUrl, userRequest);
                userResponse.EnsureSuccessStatusCode();

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var moodleUsers = JsonSerializer.Deserialize<List<MoodleUserr>>(userContent);

                if (moodleUsers == null || !moodleUsers.Any())
                {
                    return new MoodleAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Moodle user not found"
                    };
                }

                // 2. Return proper Moodle URLs
                return new MoodleAuthResult
                {
                    Success = true,
                    MoodleUser = moodleUsers[0],
                    RedirectUrl = $"{siteUrl}/login/index.php",
                    DashboardUrl = $"{siteUrl}/my"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Moodle authentication failed for {Username}", username);
                return new MoodleAuthResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    public class MoodleUserr
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("firstname")]
        public string Firstname { get; set; }

        [JsonPropertyName("lastname")]
        public string Lastname { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }

    public class MoodleAuthResult
    {
        public bool Success { get; set; }
        public MoodleUserr MoodleUser { get; set; }
        public string RedirectUrl { get; set; }
        public string DashboardUrl { get; set; }
        public string ErrorMessage { get; set; }
    }
}