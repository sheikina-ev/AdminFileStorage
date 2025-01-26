using AdminFileStorage.Models;
using AdminFileStorage.Utils;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;

namespace AdminFileStorage.Views;

public partial class UserListPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

    public UserListPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        _token = UserData.Token;

        BindingContext = this; // Устанавливаем контекст данных
        LoadUsers(); // Загружаем список пользователей
    }

    // Метод загрузки пользователей
    private async void LoadUsers()
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.GetAsync("http://course-project-4/api/users");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(responseContent);

                if (users != null)
                {
                    Users.Clear();
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось загрузить список пользователей. Код: {response.StatusCode}, Ответ: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }


    // Обработчик нажатия кнопки "Просмотр"
    private async void OnViewUserClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int userId)
        {
            await Navigation.PushAsync(new UserDetailPage(userId)); // Переход на страницу деталей
        }
    }

    // Обработчик нажатия кнопки "Удалить"
    private async void OnDeleteUserClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int userId)
        {
            var confirm = await DisplayAlert("Подтверждение", "Вы уверены, что хотите удалить этого пользователя?", "Да", "Нет");
            if (!confirm) return;

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.DeleteAsync($"http://course-project-4/api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    // Удаляем пользователя из списка
                    var userToRemove = Users.FirstOrDefault(u => u.Id == userId);
                    if (userToRemove != null)
                    {
                        Users.Remove(userToRemove);
                    }

                    await DisplayAlert("Успех", "Пользователь удалён.", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить пользователя.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
    }
}
