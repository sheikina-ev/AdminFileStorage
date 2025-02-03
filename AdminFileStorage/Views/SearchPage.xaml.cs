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
        private readonly User _currentUser;
        private readonly string _token;
        private readonly HttpClient _httpClient;

        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public ObservableCollection<File> Files { get; set; } = new ObservableCollection<File>();

        public SearchPage()
        {
            InitializeComponent();

            _currentUser = UserData.User;
            _token = UserData.Token;
            _httpClient = new HttpClient();

            BindingContext = this;
        }

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

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

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
                var userInfo =
                    $"Фамилия: {selectedUser.Surname}\n" +
                    $"Имя: {selectedUser.Name}\n" +
                    $"Имя пользователя: {selectedUser.Username}\n" +
                    $"Email: {selectedUser.Email}\n" +
                    $"Телефон: {selectedUser.Phone}";

                await DisplayAlert("Информация о пользователе", userInfo, "OK");

                ((ListView)sender).SelectedItem = null;
            }
        }

        private async void OnSearchFileButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var fileName = FileSearchEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    await DisplayAlert("Ошибка", "Введите название файла.", "OK");
                    return;
                }

                var requestBody = new { name = fileName };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                var response = await _httpClient.PostAsync($"http://course-project-4/api/search/file", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<Dictionary<string, List<File>>>(responseContent);

                    // Очистка перед обновлением списка
                    FilesFlexLayout.Children.Clear();

                    if (result != null && result.TryGetValue("files", out var files) && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            Console.WriteLine($"Файл найден: {file.Name}.{file.Extension}");

                            // Создаем контейнер для картинки и текста
                            var fileContainer = new StackLayout
                            {
                                Orientation = StackOrientation.Vertical,    // Размещение элементов вертикально
                                HorizontalOptions = LayoutOptions.Center,   // Центрируем контейнер
                                VerticalOptions = LayoutOptions.Center,     // Центрируем контейнер
                                Spacing = 5
                            };

                            // Добавление иконки файла
                            var fileIcon = new Image
                            {
                                Source = "file_icon.png",
                                WidthRequest = 50,
                                HeightRequest = 50,
                                BackgroundColor = Colors.Transparent,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            };

                            // Добавление метки с названием файла
                            var fileLabel = new Label
                            {
                                Text = $"{file.Name}.{file.Extension}",
                                TextColor = Colors.White,
                                FontSize = 18,
                                HorizontalTextAlignment = TextAlignment.Center,
                                Margin = new Thickness(0, 5)
                            };

                            // Добавление обработчика нажатия
                            var tapGesture = new TapGestureRecognizer();
                            tapGesture.Command = new Command(async (param) => await OnFileSelected(param as File));
                            tapGesture.CommandParameter = file;
                            fileLabel.GestureRecognizers.Add(tapGesture);

                            // Добавление элементов в контейнер
                            fileContainer.Children.Add(fileIcon);
                            fileContainer.Children.Add(fileLabel);

                            // Добавление контейнера в FlexLayout
                            FilesFlexLayout.Children.Add(fileContainer);
                        }
                    }
                    else
                    {
                        FilesFlexLayout.Children.Add(new Label
                        {
                            Text = "Файлы не найдены.",
                            TextColor = Colors.White,
                            FontSize = 18,
                            HorizontalTextAlignment = TextAlignment.Center
                        });
                    }
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось получить файлы.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        private async Task OnFileSelected(File file)
        {
            if (file == null)
            {
                await DisplayAlert("Ошибка", "Не удалось загрузить информацию о файле.", "OK");
                return;
            }

            await DisplayAlert("Информация о файле",
                $"Имя: {file.Name}\n" +
                $"Расширение: {file.Extension}\n" +
                $"Размер: {file.Size}\n" +
                $"Путь: {file.Path}\n" +
                $"Дата создания: {file.CreatedAt?.ToString("g") ?? "Не указана"}",
                "OK");
        }
    }
}