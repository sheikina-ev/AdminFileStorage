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

        // �������� ��� �������� ������
        public User User { get; set; } = new User();
        public List<File> Files { get; set; } = new List<File>(); // ������ ������
        public bool HasNoFiles => Files == null || Files.Count == 0;

        // ����������� ��������
        public UserDetailPage(int userId)
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _token = UserData.Token; // ��������� ������ ��������������
            _userId = userId;
            BindingContext = this; // ������������� �������� ��������

            LoadUser(); // ��������� ������ ������������
        }

        // �������� ������ ������������ � ������
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

                    // �������������� ������ ������������
                    User = JsonSerializer.Deserialize<User>(jsonObject.RootElement.GetProperty("user").GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // �������������� ������ ������
                    Files = JsonSerializer.Deserialize<List<File>>(jsonObject.RootElement.GetProperty("files").GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<File>();

                    // ���������� ��������
                    OnPropertyChanged(nameof(User));
                    OnPropertyChanged(nameof(Files));
                    OnPropertyChanged(nameof(HasNoFiles)); // ��������� ��������� ���������

                    // ��������� �����
                    LoadUserFiles();
                }
                else
                {
                    await DisplayAlert("������", "�� ������� ��������� ������ ������������.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", ex.Message, "OK");
            }
        }

        // ����� ��� ����������� ������ ������������
        private void LoadUserFiles()
        {
            // ������� ��������� ������
            FilesContainer.Children.Clear();

            // ���� ������ ���, ���������� ���������
            if (!Files.Any())
            {
                FilesContainer.Children.Add(new Label
                {
                    Text = "��� ��������� ������.",
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 16
                });
                return;
            }

            // ��������� ����� � ���������
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

        // ����� ��� ����������� ���������� � ��������� �����
        private async void OnFileSelected(File file)
        {
            await DisplayAlert("���������� � �����",
                $"���: {file.Name}\n" +
                $"����������: {file.Extension}\n" +
                $"������: {file.Size}\n" +
                $"����: {file.Path}\n" +
                $"���� ��������: {file.CreatedAt?.ToString("g") ?? "�� �������"}",
                "OK");
        }

        // ����� ��� ���������� ��������� ������ ������������
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // ������������ ������ ��� ����������
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

                // �������� ������� � ������� PATCH
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"http://course-project-4/api/users/{_userId}")
                {
                    Content = content
                };

                // ��������� ������ �����������
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                // ���������� �������
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("�����", "������ ������������ ���������.", "OK");
                    LoadUser(); // ������������ ���������� ������
                }
                else
                {
                    await DisplayAlert("������", "�� ������� �������� ������ ������������.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", ex.Message, "OK");
            }
        }
    }
}
