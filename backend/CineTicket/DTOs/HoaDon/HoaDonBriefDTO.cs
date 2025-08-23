namespace CineTicket.DTOs.HoaDon
{
    namespace CineTicket.DTOs.HoaDon
    {
        public class HoaDonBriefDTO
        {
            public int MaHd { get; set; }
            public DateTime? NgayMua { get; set; }             // = HoaDon.NgayLap
            public List<string> GheDaMua { get; set; } = new(); // danh sách số ghế (A1, A2, ...)
            public List<string> ComboBapNuoc { get; set; } = new(); // tên combo; nếu có số lượng >1 sẽ ghi "Tên (xN)"
        }
    }

}
