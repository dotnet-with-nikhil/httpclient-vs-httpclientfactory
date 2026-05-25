using HttpClient_vs_HttpClientFactory.Models;
using System.Text.Json;

namespace HttpClient_vs_HttpClientFactory.Services
{
    public class UserServiceHttpClient
    {

        //https://jsonplaceholder.typicode.com/users
        // Static HttpClient reused everywhere
        private static readonly HttpClient _client = new();

        public async Task<List<User>?> GetUsersAsync()
        {
            var response = await _client.GetAsync("https://jsonplaceholder.typicode.com/users");
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
