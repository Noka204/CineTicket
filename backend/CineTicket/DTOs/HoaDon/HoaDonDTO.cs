using CineTicket.DTOs.ChiTietHoaDon;

namespace CineTicket.DTOs.HoaDon
{
    public class HoaDonDTO
    {
        public int MaHd { get; set; }
        public DateTime? NgayLap { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }

        public string? HinhThucThanhToan { get; set; }
        public List<ChiTietHoaDonDTO> ChiTietHoaDons { get; set; } = new();
    }

}