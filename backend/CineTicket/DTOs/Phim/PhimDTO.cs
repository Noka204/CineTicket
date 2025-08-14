using CineTicket.DTOs.LoaiPhim;

public class PhimDTO
{
    public int MaPhim { get; set; }
    public string TenPhim { get; set; } = null!;
    public int? ThoiLuong { get; set; }
    public string? NgonNgu { get; set; }
    public string? DienVien { get; set; }
    public string? KhoiChieu { get; set; }
    public string? DaoDien { get; set; }
    public string? MoTa { get; set; }
    public string? Poster { get; set; }
    public string? Trailer { get; set; }
    public string? Banner { get; set; }
    public string IsHot { get; set; } = "0";

    // ✅ Danh sách loại phim (có MaLoaiPhim và TenLoaiPhim)
    public List<LoaiPhimDTO> LoaiPhims { get; set; } = new();
}
