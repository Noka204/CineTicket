namespace CineTicket.DTOs
{
    public class CreateChiTietLoaiPhimDTO
    {
        public int MaPhim { get; set; }
        public List<int> DanhSachMaLoaiPhim { get; set; } = new();
    }
}
