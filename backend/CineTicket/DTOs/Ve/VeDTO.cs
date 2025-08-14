namespace CineTicket.DTOs
{
    public class VeDTO
    {
        public int MaVe { get; set; }
        public int? MaGhe { get; set; }
        public string? SoGhe { get; set; } 

        public int? MaSuat { get; set; }
        public DateTime? ThoiGianBatDau { get; set; } 
        public decimal? GiaVe { get; set; }
        public string? TrangThai { get; set; }
    }
}
