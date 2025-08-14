using System;

namespace CineTicket.Models;

public partial class ChiTietLoaiPhim
{
    public int MaPhim { get; set; }
    public int MaLoaiPhim { get; set; }

    public virtual Phim Phim { get; set; } = null!;
    public virtual LoaiPhim LoaiPhim { get; set; } = null!;
}
