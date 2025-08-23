namespace CineTicket.DTOs.HoaDon;

public record ChiTietHoaDonOutDto(
    int MaCthd,
    int? MaVe,
    int? MaBn,
    int SoLuong,
    decimal? DonGia
);

public record HoaDonOutDto(
    int MaHd,
    DateTime? NgayLap,
    decimal? TongTien,
    string TrangThai,
    string HinhThucThanhToan,
    int MaSuat,
    string? ClientToken,
    IEnumerable<ChiTietHoaDonOutDto> ChiTietHoaDons
);
