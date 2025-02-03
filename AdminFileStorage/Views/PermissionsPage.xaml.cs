using AdminFileStorage.Utils;
using FIleStorage.Models;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminFileStorage.Views
{
    public partial class PermissionsPage : ContentPage
    {
        private readonly string _token;
        private readonly HttpClient _httpClient;

        // Списки для предоставленных и полученных прав
        public List<AccessRight> PermsGiven { get; set; } = new List<AccessRight>();
        public List<AccessRight> PermsReceived { get; set; } = new List<AccessRight>();

        public PermissionsPage()
        {
            InitializeComponent();

            // Инициализация токена пользователя
            _token = UserData.Token;

            // Настройка HttpClient
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            // Устанавливаем привязку данных
            BindingContext = this;

            // Загружаем данные о правах доступа
            LoadPermissions();
        }

        // Загрузка предоставленных и полученных прав
        private async Task LoadPermissions()
        {
            await GetProvidedPermissions();
            await GetReceivedPermissions();
        }

        // Получение предоставленных прав
        private async Task GetProvidedPermissions()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://course-project-4/api/access_rights/perms");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var permissions = JsonSerializer.Deserialize<List<AccessRight>>(jsonResponse);
                    PermsGiven = permissions;
                    OnPropertyChanged(nameof(PermsGiven)); // Уведомление привязки

                    Debug.WriteLine($"PermsGiven Loaded: {permissions.Count} items.");
                }
                else
                {
                    Debug.WriteLine("Error loading provided permissions.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Ошибка", "Ошибка при загрузке предоставленных прав доступа.", "OK");
            }
        }

        // Получение полученных прав
        private async Task GetReceivedPermissions()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://course-project-4/api/access_rights/perms/my");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var permissions = JsonSerializer.Deserialize<List<AccessRight>>(jsonResponse);
                    PermsReceived = permissions;
                    OnPropertyChanged(nameof(PermsReceived)); // Уведомление привязки

                    Debug.WriteLine($"PermsReceived Loaded: {permissions.Count} items.");
                }
                else
                {
                    Debug.WriteLine("Error loading received permissions.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Ошибка", "Ошибка при загрузке полученных прав доступа.", "OK");
            }
        }

        // Метод для добавления доступа к файлу
        private async void OnAddAccessClicked(object sender, EventArgs e)
        {
            try
            {
                var username = UsernameEntry.Text?.Trim();
                var fileIdText = FileIdEntry.Text?.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fileIdText) || !long.TryParse(fileIdText, out var fileId))
                {
                    await DisplayAlert("Ошибка", "Введите корректные данные для никнейма и ID файла.", "OK");
                    return;
                }

                var requestBody = new { username, file_id = fileId };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"http://course-project-4/api/files/{fileId}/accesses", content);

                await HandleResponse(response, "Доступ предоставлен");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        // Метод для удаления доступа к файлу
        private async void OnRemoveAccessClicked(object sender, EventArgs e)
        {
            try
            {
                var username = UsernameEntry.Text?.Trim();
                var fileIdText = FileIdEntry.Text?.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fileIdText) || !long.TryParse(fileIdText, out var fileId))
                {
                    await DisplayAlert("Ошибка", "Введите корректные данные для никнейма и ID файла.", "OK");
                    return;
                }

                var requestBody = new { username, file_id = fileId };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Delete, $"http://course-project-4/api/files/{fileId}/accesses")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);

                await HandleResponse(response, "Доступ удалён");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        // Универсальный метод для обработки ответа от API
        private async Task HandleResponse(HttpResponseMessage response, string successMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

                await DisplayAlert("Успех", $"{successMessage}:\n{result["message"]}", "OK");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                string errorMessage;
                try
                {
                    var errorData = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                    errorMessage = errorData?["error"]?.ToString() ?? "Неизвестная ошибка.";
                }
                catch
                {
                    errorMessage = "Ошибка обработки ответа сервера.";
                }

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        errorMessage = "Файл с указанным ID не найден.";
                        break;
                    case System.Net.HttpStatusCode.Unauthorized:
                        errorMessage = "Вы не авторизованы. Пожалуйста, войдите в систему.";
                        break;
                }

                await DisplayAlert("Ошибка", errorMessage, "OK");
            }
        }
    }
}
