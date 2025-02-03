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
                $"Размер: {file.Size}\n" +
                $"Путь: {file.Path}\n" +
                $"Дата создания: {file.CreatedAt?.ToString("g") ?? "Не указана"}",
                "OK");
        }

        private async void OnDeleteFileClicked(object sender, EventArgs e)
        {
            // Получаем объект файла из BindingContext кнопки
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
    }
}
