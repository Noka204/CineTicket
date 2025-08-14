namespace CineTicket.DTOs.Ve
{
    public class DatNhieuGheRequest
    {
        public int MaSuat { get; set; }
        public List<int> DanhSachMaGhe { get; set; } = new();
        public Dictionary<int, decimal>? GiaTungGhe { get; set; }
    }
}
