// ==============================
// File: DTOs/HoaDon/MyPaidMovieItemDto.cs
// (đồng bộ với HoaDonService.GetMyPaidMoviesAsync)
// ==============================
namespace CineTicket.DTOs.HoaDon
{
    public class MyPaidMovieItemDto
    {
        public int MaHd { get; set; }
        public DateTime? NgayLap { get; set; }
        public string? HinhThucThanhToan { get; set; }
        public decimal? TongTien { get; set; }

        public int MaPhim { get; set; }
        public string? TenPhim { get; set; }
        public string? Poster { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public string? GioChieu { get; set; }
    }
}
