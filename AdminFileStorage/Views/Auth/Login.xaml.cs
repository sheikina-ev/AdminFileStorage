using AdminFileStorage.Models;
using AdminFileStorage.Utils;
using System.Text;
using System.Text.Json;
using AdminFileStorage.Views;

namespace AdminFileStorage.Views.Auth;

public partial class Login : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public Login()
    {
        InitializeComponent();
    }



    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Ошибка", "Введите имя пользователя и пароль", "ОК");
            return;
        }

        var loginResponse = await AuthenticateUserAsync(username, password);

        if (loginResponse != null)
        {
            // Сохраняем данные пользователя и токен
            UserData.User = loginResponse.User;
            UserData.Token = loginResponse.Token;

            // Включаем навигацию
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;

            // Перенаправляем на страницу профиля
            await Navigation.PushAsync(new UserProfile());
        }
    }

    private async Task<AuthResponse> AuthenticateUserAsync(string username, string password)
    {
        var loginData = new { username, password };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("http://course-project-4/api/login", jsonContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);

                if (result?.Token != null)
                {
                    return result;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Ошибка входа", "Неправильный логин или пароль", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Произошла ошибка: " + response.ReasonPhrase, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
        return null;
    }
}
