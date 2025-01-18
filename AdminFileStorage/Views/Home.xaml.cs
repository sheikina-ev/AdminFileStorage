using AdminFileStorage.Models;
using AdminFileStorage.Utils;
using AdminFileStorage.Views.Auth;

namespace AdminFileStorage.Views;

public partial class Home : ContentPage
{
    private User _user;
    private string _token;
    public Home()
    {
        InitializeComponent();
        _user = UserData.User;
        _token = UserData.Token;
    }
}