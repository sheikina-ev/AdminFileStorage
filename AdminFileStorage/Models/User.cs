using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdminFileStorage.Models
{
    public class User
    {
        // Класс описывающий таблицу users
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; } = null;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; } = null;

        [JsonPropertyName("token")]
        public static string Token { get; set; }

        [JsonPropertyName("role_id")]
        public ulong RoleId { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("update_at")]
        public DateTime UpdatedAt { get; set; }
    }
}