namespace CineTicket.DTOs.LoaiPhim
{
    public class UpdateLoaiPhimRequest
    {
        public int MaLoaiPhim { get; set; }
        public string TenLoaiPhim { get; set; } = null!;
    }
}
