namespace CineTicket.DTOs
{
    public class SuatChieuDTO
    {
        public int MaSuat { get; set; }
        public int? MaPhim { get; set; }
        public string? TenPhim { get; set; } // từ MaPhimNavigation

        public int? MaPhong { get; set; }
        public string? TenPhong { get; set; } // từ MaPhongNavigation

        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public DateOnly? NgayChieu { get; set; }
        public int? MaRap { get; set; }
        public string? TenRap { get; set; } // từ MaPhongNavigation -> Raps
        public string? GioChieu { get; set; } // ✅ Thêm để hiển thị "14:30"
    }
}
