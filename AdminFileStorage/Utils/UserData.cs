using AdminFileStorage.Models;
using System.Text.Json;

namespace AdminFileStorage.Utils
{
    public static class UserData
    {
        private static string? _token = null;
        private static User? _user= null;

        public static string Token 
        { 
            get => _token ??= Preferences.Get("token", "");
        
            set
            {
                _token = value;
                Preferences.Set("token", value);
            } 
        }

        public static User User  
        { 
            get => _user ??= JsonSerializer.Deserialize<User>(Preferences.Get("user", ""));
        
            set
            {
                _user = value;
                Preferences.Set("user", JsonSerializer.Serialize(value));
            }
        }
    }
}
