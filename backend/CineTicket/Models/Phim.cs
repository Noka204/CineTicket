using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class Phim
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

    public int? MaLoaiPhim { get; set; }
    public string IsHot { get; set; } = "0";

    public virtual ICollection<ChiTietLoaiPhim> ChiTietLoaiPhims { get; set; } = new List<ChiTietLoaiPhim>();

    public virtual ICollection<SuatChieu> SuatChieus { get; set; } = new List<SuatChieu>();
}
