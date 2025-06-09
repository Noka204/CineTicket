namespace CineTicket.DTOs
{
    public class PhimDTO
    {
        public int MaPhim { get; set; }
        public string TenPhim { get; set; } = null!;
        public int? ThoiLuong { get; set; }
        public string? DaoDien { get; set; }
        public string? MoTa { get; set; }
        public string? Poster { get; set; }
        public int? MaLoaiPhim { get; set; }
        public string? TenLoaiPhim { get; set; } // từ MaLoaiPhimNavigation
    }
}
