namespace CineTicket.DTOs
{
    public class ChiTietLoaiPhimDTO
    {
        public int MaPhim { get; set; }
        public int MaLoaiPhim { get; set; }

        // Tuỳ chọn: hiển thị thêm tên phim và tên loại phim
        public string? TenPhim { get; set; }
        public string? TenLoaiPhim { get; set; }
    }
}
