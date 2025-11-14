// ==============================
// File: DTOs/KhuyenMai/KhuyenMaiOutDto.cs
// ==============================
using CineTicket.Models;

namespace CineTicket.DTOs.KhuyenMai
{
    public class KhuyenMaiOutDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = null!;
        public bool IsActive { get; set; }
        public DiscountKind LoaiGiam { get; set; }
        public decimal MucGiam { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }

        // tuỳ chọn hiển thị thêm
        public int SoCode { get; set; }
    }
}
    