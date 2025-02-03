using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using AdminFileStorage.Models;

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

            throw new Exception("Failed to load user files");
        }

        public async Task<bool> DeleteUserFileAsync(int fileId)
        {
            try
            {
                var url = $"http://course-project-4/api/files/{fileId}";
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                Console.WriteLine($"Ошибка при удалении файла. Статус код: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении файла: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UploadFileAsync(string filePath)
        {
            try
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath); 

                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var url = "http://course-project-4/api/files";
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = fileContent
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
                return false;
            }
        }
    }
}
