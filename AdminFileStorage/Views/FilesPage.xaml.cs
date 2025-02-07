using AdminFileStorage.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Net.Http.Headers;
using File = AdminFileStorage.Models.File;
using Newtonsoft.Json;
using AdminFileStorage.Utils;
using FIleStorage.Models;

namespace AdminFileStorage.Views
{
    public partial class FilesPage : ContentPage
    {
        private readonly UserService _userService;
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public FilesPage()
        {
            InitializeComponent();
            _token = UserData.Token;
            _httpClient = new HttpClient();
            _userService = new UserService(_httpClient, _token);

            LoadUserFiles();
        }

        private async void LoadUserFiles()
        {
            try
            {
                var files = await _userService.GetUserFilesAsync();
                FilesCollectionView.ItemsSource = files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки файлов: {ex.Message}");
                await DisplayAlert("Ошибка", "Не удалось загрузить файлы.", "OK");
            }
        }

        private async void OnFileSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is File selectedFile)
            {
                await ShowFileDetails(selectedFile);
            }
        }

        private async Task ShowFileDetails(File file)
        {
            await DisplayAlert("Информация о файле",
                $"Имя: {file.Name}\n" +
                $"Расширение: {file.Extension}\n" +
                $"Размер: {file.Size} KB\n" +
                $"Путь: {file.Path}",
                "OK");
        }

        private async void OnDeleteFileClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is File file)
            {
                bool confirm = await DisplayAlert("Удаление файла", $"Вы уверены, что хотите удалить {file.Name}?", "Да", "Нет");
                if (confirm)
                {
                    await DeleteFile(file);
                }
            }
        }

        private async Task DeleteFile(File file)
        {
            try
            {
                bool success = await _userService.DeleteUserFileAsync(file.Id);
                if (success)
                {
                    await DisplayAlert("Успех", "Файл успешно удален", "OK");
                    LoadUserFiles();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить файл", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления файла: {ex.Message}");
                await DisplayAlert("Ошибка", "Произошла ошибка при удалении файла", "OK");
            }
        }

        private async void OnDownloadFileClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is File file)
            {
                bool confirm = await DisplayAlert("Скачать файл", $"Вы хотите скачать {file.Name}?", "Да", "Нет");
                if (confirm)
                {
                    await DownloadFile(file);
                }
            }
        }

        private async Task DownloadFile(File file)
        {
            try
            {
                string downloadUrl = $"https://yourapi.com/files/download/{file.Id}";
                var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                request.Headers.Add("Authorization", $"Bearer {_token}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();

                    // Путь к папке Загрузки (Downloads)
                    string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    string localFilePath = Path.Combine(downloadsPath, $"{file.Name}.{file.Extension}");

                    // Записываем файл
                    await System.IO.File.WriteAllBytesAsync(localFilePath, fileBytes);

                    await DisplayAlert("Успех", $"Файл сохранён: {localFilePath}", "OK");

                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось скачать файл.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки файла: {ex.Message}");
                await DisplayAlert("Ошибка", "Произошла ошибка при скачивании файла.", "OK");
            }
        }
        // Обработчик для кнопки загрузки нового файла
        private async void OnUploadFileClicked(object sender, EventArgs e)
        {
            try
            {
                // Открываем диалог выбора файла
                var filePickerResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images // Или любые другие файлы, которые хотите поддерживать
                });

                if (filePickerResult != null)
                {
                    // Получаем путь к файлу
                    var filePath = filePickerResult.FullPath;
                    var fileName = filePickerResult.FileName;

                    // Теперь отправим файл на сервер
                    await UploadFileToServer(filePath, fileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выбора файла: {ex.Message}");
                await DisplayAlert("Ошибка", "Произошла ошибка при выборе файла.", "OK");
            }
        }

        private async Task UploadFileToServer(string filePath, string fileName)
        {
            try
            {
                var fileContent = new MultipartFormDataContent();
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath); // Чтение файла
                fileContent.Add(new ByteArrayContent(fileBytes), "file", fileName);

                var uploadUrl = "http://course-project-4/api/files"; // Ваш роут
                var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = fileContent
                };
                request.Headers.Add("Authorization", $"Bearer {_token}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Успех", "Файл успешно загружен!", "OK");
                    LoadUserFiles(); // Обновляем список файлов
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось загрузить файл.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки файла: {ex.Message}");
                await DisplayAlert("Ошибка", "Произошла ошибка при загрузке файла.", "OK");
            }
        }
    }
}

