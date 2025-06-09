using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class SuatChieu
{
    public int MaSuat { get; set; }

    public int? MaPhim { get; set; }

    public int? MaPhong { get; set; }

    public DateTime? ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public DateOnly? NgayChieu { get; set; }

    public virtual Phim? MaPhimNavigation { get; set; }

    public virtual PhongChieu? MaPhongNavigation { get; set; }

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
