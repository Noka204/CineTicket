using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class Phim
{
    public int MaPhim { get; set; }

    public string TenPhim { get; set; } = null!;

    public int? ThoiLuong { get; set; }

    public string? DaoDien { get; set; }

    public string? MoTa { get; set; }

    public string? Poster { get; set; }

    public int? MaLoaiPhim { get; set; }

    public virtual LoaiPhim? MaLoaiPhimNavigation { get; set; }

    public virtual ICollection<SuatChieu> SuatChieus { get; set; } = new List<SuatChieu>();
}
