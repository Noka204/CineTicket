// ==============================
// File: DTOs/HoaDon/CreateHoaDonDTO.cs
// ==============================
using System.ComponentModel.DataAnnotations;

namespace CineTicket.DTOs.HoaDon
{
    public class CreateHoaDonDTO
    {
        [Required]
        public int MaSuat { get; set; }

        // FE có thể gửi seatIds (MaGhe) hoặc ChiTietHoaDons với MaVe
        public List<int>? SeatIds { get; set; }

        // Dòng chi tiết: (MaVe, SoLuong=1) hoặc (MaBn, SoLuong)
        public List<CreateHoaDonLineDTO>? ChiTietHoaDons { get; set; }

        public string? HinhThucThanhToan { get; set; } // "Momo" | "VNPAY" ...
        public string? GhiChu { get; set; }

        // tuỳ ý: nếu muốn truyền mã KM từ checkout
        public string? CouponCode { get; set; }
    }

    public class CreateHoaDonLineDTO
    {
        public int? MaVe { get; set; }
        public int? MaBn { get; set; }
        public int SoLuong { get; set; } = 1;
    }
}
