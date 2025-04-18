using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace StyleDetectionTool.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetDataFromApiAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Gọi API thất bại: {response.StatusCode}");
            }

            var contentStream = await response.Content.ReadAsStreamAsync();

            // Deserialize JSON array thành List<string>
            var urlList = await JsonSerializer.DeserializeAsync<List<string>>(contentStream);

            return urlList ?? new List<string>();
        }
    }
}
