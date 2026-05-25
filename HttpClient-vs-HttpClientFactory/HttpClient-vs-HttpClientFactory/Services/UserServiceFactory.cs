using HttpClient_vs_HttpClientFactory.Models;
using System.Text.Json;

namespace HttpClient_vs_HttpClientFactory.Services
{
    public class UserServiceFactory
    {
        private readonly HttpClient _client;

        //HttpClient can injected automatically
        public UserServiceFactory(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<User>?> GetUsersAsync()
        {
            var response = await _client.GetAsync("users");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var users = JsonSerializer.Deserialize<List<User>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
             );
            return users;
        }
    }
}
