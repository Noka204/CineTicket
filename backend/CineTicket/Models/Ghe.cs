using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class Ghe
{
    public int MaGhe { get; set; }

    public string? SoGhe { get; set; }

    public string? LoaiGhe { get; set; }

    public int? MaPhong { get; set; }
    public string T { get; set; }

    public virtual Ve? VeNavigation { get; set; }

    public virtual PhongChieu? MaPhongNavigation { get; set; }

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
