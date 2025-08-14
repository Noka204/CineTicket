using CineTicket.DTOs.ChiTietHoaDon;

namespace CineTicket.DTOs.HoaDon
{
    public class CreateHoaDonDTO
    {
        public int MaSuat { get; set; }
        public List<int> SeatIds { get; set; } = new(); // danh sách MaGhe đã hold
        public int? BapNuocId { get; set; }
        public int SoLuongBapNuoc { get; set; } = 0;
        public string? GhiChu { get; set; }
        public string? HinhThucThanhToan { get; set; } // "Momo" mặc định
        public string? ClientToken { get; set; }        // chống tạo HĐ trùng
        public List<CreateChiTietHoaDonDTO> ChiTietHoaDons { get; set; } = new();
    }
}
