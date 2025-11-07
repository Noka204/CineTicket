using Microsoft.EntityFrameworkCore;
public class MyPaidMovieItemDto
{
    public int MaHd { get; set; }
    public DateTime? NgayLap { get; set; }
    public decimal? TongTien { get; set; }
    // Phim
    public int MaPhim { get; set; }
    public string? TenPhim { get; set; }
    public string? Poster { get; set; }

    // Suất chiếu
    public DateTime? ThoiGianBatDau { get; set; }
    public string? GioChieu { get; set; }

    // Phương thức thanh toán
    public string? HinhThucThanhToan { get; set; }
}

