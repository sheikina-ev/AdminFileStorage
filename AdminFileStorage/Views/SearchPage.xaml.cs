using AdminFileStorage.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using File = AdminFileStorage.Models.File;
using AdminFileStorage.Utils;

namespace AdminFileStorage.Views
{
    public partial class SearchPage : ContentPage
    {
        private readonly User _currentUser; // Текущий пользователь
        private readonly string _token;    // Токен авторизации
        private readonly HttpClient _httpClient;

        // Список пользователей для поиска
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        // Список файлов текущего пользователя
        public ObservableCollection<File> Files { get; set; } = new ObservableCollection<File>();

        public SearchPage()
        {
            InitializeComponent();

            _currentUser = UserData.User;
            _token = UserData.Token;
            _httpClient = new HttpClient();

            BindingContext = this; // Устанавливаем контекст данных
        }

        // Поиск пользователей по имени
        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var username = SearchEntry.Text;
                if (string.IsNullOrWhiteSpace(username))
                {
                    await DisplayAlert("Ошибка", "Введите имя пользователя.", "OK");
                    return;
                }

                var requestBody = new { username };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Устанавливаем заголовок с токеном
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                // Запрос на поиск пользователя по имени
                var response = await _httpClient.PostAsync("http://course-project-4/api/search/user", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, List<User>>>(responseContent);

                    if (result != null && result.TryGetValue("users", out var users) && users.Count > 0)
                    {
                        Users.Clear();
                        foreach (var user in users)
                        {
                            Users.Add(user);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Результаты поиска", "Пользователи не найдены.", "OK");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Ошибка", "Пользователи не были найдены!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }
        private async void OnUserSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is User selectedUser)
            {
                // Формируем строку с информацией о пользователе
                var userInfo =
                    $"Фамилия: {selectedUser.Surname}\n" +
                    $"Имя: {selectedUser.Name}\n" +
                    $"Имя пользователя: {selectedUser.Username}\n" +
                    $"Email: {selectedUser.Email}\n" +
                    $"Телефон: {selectedUser.Phone}";

                // Отображаем модальное окно
                await DisplayAlert("Информация о пользователе", userInfo, "OK");

                // Снимаем выделение
                ((ListView)sender).SelectedItem = null;
            }
        }


        // Поиск файлов у текущего пользователя
        private async void OnSearchFileButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Получаем введенное имя файла
                var fileName = FileSearchEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    await DisplayAlert("Ошибка", "Введите название файла.", "OK");
                    return;
                }

                // Подготовка тела запроса
                var requestBody = new { name = fileName };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Установка токена авторизации
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                // Выполнение POST-запроса
                var response = await _httpClient.PostAsync($"http://course-project-4/api/search/file", content);

                // Получение ответа
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Десериализация ответа
                    var result = JsonSerializer.Deserialize<Dictionary<string, List<File>>>(responseContent);

                    if (result != null && result.TryGetValue("files", out var files) && files.Count > 0)
                    {
                        // Очистка текущего списка и добавление новых файлов
                        Files.Clear();
                        foreach (var file in files)
                        {
                            Files.Add(file);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Результаты поиска", "Файлы не найдены.", "OK");
                    }
                }
                else
                {
                    // Логирование ошибки, если запрос не удался
                    await DisplayAlert("Ошибка", $"API вернул ошибку: {response.StatusCode}\n{responseContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }



    }
}
