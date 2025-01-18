using AdminFileStorage.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using AdminFileStorage.Views.Auth;
using AdminFileStorage.Utils;

namespace AdminFileStorage.Views
{
    public partial class UserUpdateProfilePage : ContentPage
    {
        private readonly User _user;
        private readonly string _token;
        private readonly HttpClient _httpClient;

        public event Action<User> UserProfileUpdated; // Событие для обновления профиля

        public UserUpdateProfilePage()
        {
            InitializeComponent();
            _user = UserData.User;
            _token = UserData.Token;
            _httpClient = new HttpClient();

            LoadUserProfile();

            // Проверяем, что токен содержит правильный userId
            if (!ValidateToken(_token, _user.Id.ToString()))
            {
                Console.WriteLine("Ошибка: токен не содержит правильного userId.");
            }
        }

        private void LoadUserProfile()
        {
            // Загружаем данные в поля
            NameEntry.Text = _user.Name;
            SurnameEntry.Text = _user.Surname;
            UsernameEntry.Text = _user.Username;
            UsernameLabel.Text = _user.Username;
            EmailEntry.Text = _user.Email;
            PhoneEntry.Text = string.IsNullOrEmpty(_user.Phone) ? "" : _user.Phone;
        }

        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Заполняем объект _user новыми данными
                _user.Name = NameEntry.Text;
                _user.Surname = SurnameEntry.Text;
                _user.Username = UsernameEntry.Text;
                _user.Email = EmailEntry.Text;
                _user.Phone = PhoneEntry.Text;

                // Логируем отправляемые данные
                Console.WriteLine("Отправляемый JSON:");
                Console.WriteLine(JsonSerializer.Serialize(_user));

                // Отправляем запрос
                var success = await UpdateUserProfileAsync();

                if (success)
                {
                    // Если успешно, обновляем UI с новыми данными
                    LoadUserProfile();  // Обновление UI с новыми данными

                    UserProfileUpdated?.Invoke(_user); // Сигнализируем об обновлении
                    await DisplayAlert("Успех", "Профиль успешно обновлен", "OK");
                }
                else
                {
                    ErrorLabel.Text = "Не удалось обновить профиль. Попробуйте позже.";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                // Логируем исключение
                Console.WriteLine($"Ошибка: {ex.Message}");
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        private async Task<bool> UpdateUserProfileAsync()
        {
            try
            {
                // Создаем анонимный объект для отправки данных
                var updatePayload = new
                {
                    id = _user.Id,
                    name = _user.Name,
                    surname = _user.Surname,
                    email = _user.Email,
                    phone = _user.Phone,
                    username = _user.Username
                };

                // Сериализация объекта в JSON
                var jsonContent = new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json");

                // Устанавливаем токен авторизации
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                // Отправляем запрос
                var response = await _httpClient.PostAsync("http://course-project-4/api/profile", jsonContent);

                // Читаем ответ от сервера
                var responseContent = await response.Content.ReadAsStringAsync();

                // Логируем статус-код и ответ сервера
                Console.WriteLine("Статус-код ответа: " + response.StatusCode);
                Console.WriteLine("Ответ сервера: " + responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Если сервер вернул ошибку, показываем ее
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                ErrorLabel.Text = errorResponse?.Message ?? "Неизвестная ошибка.";
                ErrorLabel.IsVisible = true;

                return false;
            }
            catch (Exception ex)
            {
                // Логируем исключение
                Console.WriteLine($"Ошибка при обновлении профиля: {ex.Message}");
                return false;
            }
        }

        private async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            // Спрашиваем подтверждение выхода
            bool answer = await DisplayAlert("Подтверждение", "Вы действительно хотите выйти?", "Да", "Нет");
            if (answer)
            {
                // Сбрасываем данные пользователя
                UserData.User = null;
                UserData.Token = null;

                // Отключаем навигационное меню
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

                // Переход на страницу логина
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }


        private bool ValidateToken(string token, string expectedUserId)
        {
            try
            {
                // Разделяем токен на части (header, payload, signature)
                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    Console.WriteLine("Неверный формат токена.");
                    return false;
                }

                // Декодируем payload (второй элемент)
                var payload = parts[1];
                var decodedPayload = Base64UrlDecode(payload);
                var payloadJson = Encoding.UTF8.GetString(decodedPayload);

                // Преобразуем payload в объект
                var payloadObject = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);

                // Проверяем, что токен содержит ожидаемый userId
                if (payloadObject.TryGetValue("sub", out var userId))
                {
                    if (userId.ToString() == expectedUserId)
                    {
                        return true;
                    }
                }

                Console.WriteLine("Токен не содержит ожидаемого userId.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке токена: {ex.Message}");
                return false;
            }
        }

        private byte[] Base64UrlDecode(string input)
        {
            string base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }

    // Пример класса для обработки ошибок сервера
    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
