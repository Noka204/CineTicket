namespace CineTicket.DTOs
{
    public class UpdateVeRequest
    {
        public int MaVe { get; set; }
        public int? MaGhe { get; set; }
        public int? MaSuat { get; set; }
        public decimal? GiaVe { get; set; }
        public string? TrangThai { get; set; }
    }
}
