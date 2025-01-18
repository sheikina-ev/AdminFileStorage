using AdminFileStorage.Views.Auth;
namespace AdminFileStorage.Views

{
    public partial class MainPage : ContentPage
    {
     
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnStartButtonClicked(object sender, EventArgs e)
        {
            // Navigate to the Login page located in Models/Auth/Login
            //await Navigation.PushAsync(new Login());
            await Shell.Current.GoToAsync("//LoginPage");

        }
    }

}
