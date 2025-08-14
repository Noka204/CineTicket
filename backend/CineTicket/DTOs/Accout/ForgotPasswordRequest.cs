using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CineTicket.DTOs.Auth
{
    public class ForgotPasswordRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }

}
