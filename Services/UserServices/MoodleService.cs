using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Career_Tracker_Backend.Services.UserServices
{
    public class MoodleService : IMoodleService
    {
        private readonly HttpClient _httpClient;

        public MoodleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CreateMoodleUserAsync(string username, string firstname, string lastname, string password, string email)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("wstoken", moodleToken),
                new KeyValuePair<string, string>("wsfunction", "core_user_create_users"),
                new KeyValuePair<string, string>("moodlewsrestformat", "json"),
                new KeyValuePair<string, string>("users[0][username]", username),
                new KeyValuePair<string, string>("users[0][password]", password),
                new KeyValuePair<string, string>("users[0][firstname]", firstname),
                new KeyValuePair<string, string>("users[0][lastname]", lastname),
                new KeyValuePair<string, string>("users[0][email]", email),
            });

            var response = await _httpClient.PostAsync(moodleUrl, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMoodleUserAsync(List<int> userIds)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("wstoken", moodleToken),
                new KeyValuePair<string, string>("wsfunction", "core_user_delete_users"),
                new KeyValuePair<string, string>("moodlewsrestformat", "json")
            };

            for (int i = 0; i < userIds.Count; i++)
            {
                formData.Add(new KeyValuePair<string, string>($"userids[{i}]", userIds[i].ToString()));
            }

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new Exception($"Failed to delete user in Moodle. Response: {responseContent}");
            }
        }

        // Define a class to deserialize the Moodle success response
        public class MoodleDeleteUserResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("warnings")]
            public List<string> Warnings { get; set; }
        }
        public async Task<int?> GetMoodleUserIdByEmailAsync(string email)
        {
            var moodleUrl = "http://localhost/Mymoodle/webservice/rest/server.php";
            var moodleToken = "aba8b4d828c431ef68123b83f5a9cba8";

            var formData = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("wstoken", moodleToken),
        new KeyValuePair<string, string>("wsfunction", "core_user_get_users"),
        new KeyValuePair<string, string>("moodlewsrestformat", "json"),
        new KeyValuePair<string, string>("criteria[0][key]", "email"),
        new KeyValuePair<string, string>("criteria[0][value]", email)
    };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(moodleUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Deserialize Moodle response to get user ID
                var moodleUsers = JsonConvert.DeserializeObject<MoodleGetUsersResponse>(responseContent);
                var moodleUser = moodleUsers?.Users.FirstOrDefault();
                return moodleUser?.Id;
            }
            else
            {
                throw new Exception($"Failed to retrieve user from Moodle. Response: {responseContent}");
            }
        }

        // Define a class for deserializing the Moodle response
        public class MoodleGetUsersResponse
        {
            [JsonProperty("users")]
            public List<MoodleUser> Users { get; set; }
        }

        public class MoodleUser
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }
        }

    }
}