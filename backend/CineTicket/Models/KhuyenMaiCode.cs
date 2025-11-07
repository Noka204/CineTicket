using System;

namespace CineTicket.Models
{
    public class KhuyenMaiCode
    {
        public int Id { get; set; }

        public int KhuyenMaiId { get; set; }
        public KhuyenMai KhuyenMai { get; set; } = null!;

        public string Code { get; set; } = null!;         // UNIQUE, lưu uppercase

        // (optional) mã đích danh
        public string? AssignedToUserId { get; set; }

        // Đánh dấu đã dùng để chặn tái sử dụng
        public DateTime? RedeemedAt { get; set; }
    }
}
