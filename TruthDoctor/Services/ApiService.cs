using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace TruthDoctor.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string? _token;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5029/")
            };
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/v1/auth/login",
                    new { username, password }
                );

                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(content);
                var token = doc.RootElement
                    .GetProperty("data")
                    .GetProperty("token")
                    .GetString();

                if (string.IsNullOrEmpty(token))
                    return false;

                _token = token;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetValidationAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/validate/network");

                if (!response.IsSuccessStatusCode)
                    return $"Error: {response.StatusCode}";

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }
}
