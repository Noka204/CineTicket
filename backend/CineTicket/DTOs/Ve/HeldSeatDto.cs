namespace CineTicket.DTOs.Ve
{
    public class HeldSeatDto
    {
        public int MaVe { get; set; }
        public int MaGhe { get; set; }
        public int SoGhe { get; set; }
        public int MaSuat { get; set; }
        public DateTime? ThoiGianHetHan { get; set; }
        public string TrangThai { get; set; } = "TamGiu";
    }
}
