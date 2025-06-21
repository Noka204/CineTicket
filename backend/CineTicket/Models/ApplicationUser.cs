using System.ComponentModel.DataAnnotations;
using CineTicket.Models;
using Microsoft.AspNetCore.Identity;

namespace CineTicket.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Bạn có thể thêm các property custom nếu cần, ví dụ:
        public required string FullName { get; set; }
        // Quan hệ đến các đơn hàng:
        public ICollection<Ve> Ves { get; set; }
        public ICollection<HoaDon> HoaDons  { get; set; }
        public string Address { get; set; } = null!;
        public string Role { get; set; }
    }

}
