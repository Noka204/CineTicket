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
    }
}
