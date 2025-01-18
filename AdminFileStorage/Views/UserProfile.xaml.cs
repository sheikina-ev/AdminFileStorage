using AdminFileStorage.Models;
using AdminFileStorage.Utils;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

// Добавляем псевдоним для класса File
using MyFile = AdminFileStorage.Models.File;
using System.IO;

namespace AdminFileStorage.Views
{
    public partial class UserProfile : ContentPage
    {
        private User _user;
        private readonly string _token;
        private readonly HttpClient _httpClient;
        private readonly UserService _userService;

        public UserProfile()
        {
            
            InitializeComponent();


            _user = UserData.User;
            _token = UserData.Token;
            _httpClient = new HttpClient();
            _userService = new UserService(_httpClient, _token);

            LoadUserProfile();
            LoadUserFiles(); // Загружаем файлы пользователя при загрузке страницы
        }

        private void LoadUserProfile()
        {
            // Заполняем данные пользователя
            UsernameLabel.Text = _user.Username;
            UsernameStaticLabel.Text = _user.Username;
            EmailStaticLabel.Text = _user.Email;
        }

        private async void LoadUserFiles()
        {
            try
            {
                // Получаем все файлы пользователя
                var files = await _userService.GetUserFilesAsync();

                // Берем только первые три файла
                var limitedFiles = files.Take(3).ToList();

                // Очищаем контейнер
                FilesContainer.Children.Clear();

                // Если файлов нет, отображаем сообщение
                if (!limitedFiles.Any())
                {
                    FilesContainer.Children.Add(new Label
                    {
                        Text = "Нет доступных файлов. Вы можете загрузить файлы через соответствующий раздел.",
                        TextColor = Colors.White,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 16,
                        Padding = new Thickness(20)
                    });
                    return;
                }

                // Добавляем файлы в контейнер
                foreach (var file in limitedFiles)
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
            catch (Exception ex)
            {
                // Логируем ошибку в консоль или добавляем сообщение на страницу
                Console.WriteLine($"Ошибка при загрузке файлов: {ex.Message}");

                // Показываем сообщение об ошибке на странице, если требуется
                FilesContainer.Children.Clear();
                FilesContainer.Children.Add(new Label
                {
                    Text = "Нет доступных файлов",
                    TextColor = Colors.Red,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 16,
                    Padding = new Thickness(20)
                });
            }
        }



        private async void OnFileSelected(MyFile file)
        {
            await DisplayAlert("Информация о файле",
                $"Имя: {file.Name}\n" +
                $"Расширение: {file.Extension}\n" +
                $"Размер: {file.Size}\n" +
                $"Путь: {file.Path}\n" +
                $"Дата создания: {file.CreatedAt?.ToString("g") ?? "Не указана"}",
                "OK");
        }

        private async void OnEditButtonClicked(object sender, EventArgs e)
        {
            var updateProfilePage = new UserUpdateProfilePage();

            updateProfilePage.UserProfileUpdated += OnUserProfileUpdated;

            await Navigation.PushAsync(updateProfilePage);
        }

        private void OnUserProfileUpdated(User updatedUser)
        {
            _user = updatedUser;
            LoadUserProfile();
        }

        private async void OnShowAllFilesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FilesPage());
        }
        private async void OnNavigateToFilesPage(object sender, EventArgs e)
        {
            // Переход на страницу "Файлы"
            await Shell.Current.GoToAsync("//filesPage");
        }

      


    }
}
