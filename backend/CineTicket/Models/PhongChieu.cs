using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class PhongChieu
{
    public int MaPhong { get; set; }

    public string? TenPhong { get; set; }

    public int? SoGhe { get; set; }
    public int MaRap { get; set; }
    public virtual Rap? Rap { get; set; }
    public virtual ICollection<Ghe> Ghes { get; set; } = new List<Ghe>();

    public virtual ICollection<SuatChieu> SuatChieus { get; set; } = new List<SuatChieu>();
}
