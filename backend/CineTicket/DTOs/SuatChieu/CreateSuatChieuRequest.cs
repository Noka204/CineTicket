namespace CineTicket.DTOs
{
    public class CreateSuatChieuRequest
    {
        public int? MaPhim { get; set; }
        public int? MaPhong { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public DateOnly? NgayChieu { get; set; }
        public string? GioChieu { get; set; }
    }
}
