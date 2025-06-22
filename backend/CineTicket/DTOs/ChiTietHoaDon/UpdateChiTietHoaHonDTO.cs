namespace CineTicket.DTOs.ChiTietHoaDon
{
    public class UpdateHoaDonRequest
    {
        public DateTime? NgayLap { get; set; }
        public decimal? TongTien { get; set; }
        public string? HinhThucThanhToan { get; set; }
        public List<CreateChiTietHoaDonDTO>? ChiTietHoaDons { get; set; }
    }

}
