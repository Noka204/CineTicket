namespace CineTicket.DTOs
{
    public class UpdatePhongChieuRequest
    {
        public int MaPhong { get; set; }
        public string? TenPhong { get; set; }
        public int? SoGhe { get; set; }
        public int? TenGhe { get; set; }
        public int MaRap { get; set; }
    }
}
