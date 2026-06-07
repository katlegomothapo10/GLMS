using System.Text;
using System.Text.Json;

namespace GLMS.Services
{
    public interface IApiService
    {
        Task<List<T>> GetAsync<T>(string endpoint);
        Task<T> GetByIdAsync<T>(string endpoint, int id);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, int id, object data);
        Task<T> PatchAsync<T>(string endpoint, int id, object data);
        Task<bool> DeleteAsync(string endpoint, int id);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<T>> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>();
            }
            return new List<T>();
        }

        public async Task<T> GetByIdAsync<T>(string endpoint, int id)
        {
            var response = await _httpClient.GetAsync($"{endpoint}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            return default!;
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                Console.WriteLine($"POST to: {_httpClient.BaseAddress}{endpoint}");
                Console.WriteLine($"Body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {(int)response.StatusCode} - {responseJson}");

                response.EnsureSuccessStatusCode();
                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostAsync Error: {ex.Message}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, int id, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{endpoint}/{id}", content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<T> PatchAsync<T>(string endpoint, int id, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Patch, $"{endpoint}/{id}/status")
            {
                Content = content
            };
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<bool> DeleteAsync(string endpoint, int id)
        {
            var response = await _httpClient.DeleteAsync($"{endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}