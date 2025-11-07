// CineTicket.DTOs.Accout/UpdateProfileRequest.cs
using System.ComponentModel.DataAnnotations;

namespace CineTicket.DTOs.Accout
{
    public class UpdateProfileRequest
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [EmailAddress, MaxLength(256)]
        public string? Email { get; set; }      // Cho phép đổi email (không bắt buộc)

        [MaxLength(255)]
        public string? Address { get; set; }

        [Phone, MaxLength(30)]
        public string? Sdt { get; set; }        // PhoneNumber
    }
}
