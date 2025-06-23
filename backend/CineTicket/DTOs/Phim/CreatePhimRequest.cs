namespace CineTicket.DTOs
{
    public class CreatePhimRequest
    {
        public string TenPhim { get; set; }
        public int? ThoiLuong { get; set; }
        public string? DaoDien { get; set; }
        public string? MoTa { get; set; }
        public string? Poster { get; set; }
        public int? MaLoaiPhim { get; set; }
        public string? TenLoaiPhim { get; set; } // từ MaLoaiPhimNavigation
    }
}
