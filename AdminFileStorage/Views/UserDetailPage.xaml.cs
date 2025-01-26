using AdminFileStorage.Models;
using AdminFileStorage.Utils;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using File = AdminFileStorage.Models.File;

namespace AdminFileStorage.Views
{
    public partial class UserDetailPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly int _userId;

        // Свойства для привязки данных
        public User User { get; set; } = new User();
        public List<File> Files { get; set; } = new List<File>(); // Список файлов
        public bool HasNoFiles => Files == null || Files.Count == 0;

        // Конструктор страницы
        public UserDetailPage(int userId)
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _token = UserData.Token; // Получение токена администратора
            _userId = userId;
            BindingContext = this; // Устанавливаем контекст привязки

            LoadUser(); // Загружаем данные пользователя
        }

        // Загрузка данных пользователя и файлов
        private async void LoadUser()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.GetAsync($"http://course-project-4/api/users/{_userId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonDocument.Parse(responseContent);

                    // Десериализация данных пользователя
                    User = JsonSerializer.Deserialize<User>(jsonObject.RootElement.GetProperty("user").GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Десериализация списка файлов
                    Files = JsonSerializer.Deserialize<List<File>>(jsonObject.RootElement.GetProperty("files").GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<File>();

                    // Обновление привязки
                    OnPropertyChanged(nameof(User));
                    OnPropertyChanged(nameof(Files));
                    OnPropertyChanged(nameof(HasNoFiles)); // Обновляем видимость сообщения

                    // Загружаем файлы
                    LoadUserFiles();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось загрузить данные пользователя.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        // Метод для отображения файлов пользователя
        private void LoadUserFiles()
        {
            // Очищаем контейнер файлов
            FilesContainer.Children.Clear();

            // Если файлов нет, отображаем сообщение
            if (!Files.Any())
            {
                FilesContainer.Children.Add(new Label
                {
                    Text = "Нет доступных файлов.",
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 16
                });
                return;
            }

            // Добавляем файлы в контейнер
            foreach (var file in Files)
            {
                var fileStack = new VerticalStackLayout
                {
                    Spacing = 5,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                var fileButton = new ImageButton
                {
                    Source = "file_icon.png",
                    WidthRequest = 80,
                    HeightRequest = 80,
                    BackgroundColor = Colors.Transparent,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                fileButton.Clicked += (s, e) => OnFileSelected(file);

                var fileNameLabel = new Label
                {
                    Text = file.Name,
                    FontSize = 14,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };

                fileStack.Children.Add(fileButton);
                fileStack.Children.Add(fileNameLabel);

                FilesContainer.Children.Add(fileStack);
            }
        }

        // Метод для отображения информации о выбранном файле
        private async void OnFileSelected(File file)
        {
            await DisplayAlert("Информация о файле",
                $"Имя: {file.Name}\n" +
                $"Расширение: {file.Extension}\n" +
                $"Размер: {file.Size}\n" +
                $"Путь: {file.Path}\n" +
                $"Дата создания: {file.CreatedAt?.ToString("g") ?? "Не указана"}",
                "OK");
        }

        // Метод для сохранения изменений данных пользователя
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Формирование данных для обновления
                var updatedUser = new
                {
                    Surname = SurnameEntry.Text,
                    Name = NameEntry.Text,
                    Username = UsernameEntry.Text,
                    Email = EmailEntry.Text,
                    Phone = PhoneEntry.Text
                };

                var jsonContent = JsonSerializer.Serialize(updatedUser);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Создание запроса с методом PATCH
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"http://course-project-4/api/users/{_userId}")
                {
                    Content = content
                };

                // Установка токена авторизации
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                // Выполнение запроса
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Успех", "Данные пользователя обновлены.", "OK");
                    LoadUser(); // Перезагрузка актуальных данных
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось обновить данные пользователя.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
    }
}
