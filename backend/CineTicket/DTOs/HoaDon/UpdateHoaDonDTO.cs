using CineTicket.DTOs.ChiTietHoaDon;

public class UpdateHoaDonDTO
{
    public int MaHd { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TongTien { get; set; }
    public string HinhThucThanhToan { get; set; }
    public List<ChiTietHoaDonDTO> ChiTietHoaDons { get; set; } = new();
}
