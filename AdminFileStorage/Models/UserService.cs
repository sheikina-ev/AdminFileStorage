using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminFileStorage.Models
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public UserService(HttpClient httpClient, string token)
        {
            _httpClient = httpClient;
            _token = token;
        }

        public async Task<List<File>> GetUserFilesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://course-project-4/api/files/disk");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonDocument.Parse(content);
                var files = JsonSerializer.Deserialize<List<File>>(jsonObject.RootElement.GetProperty("files").ToString(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return files;
            }

            // Обработка ошибок
            throw new Exception("Failed to load user files");
        }
    }
}
