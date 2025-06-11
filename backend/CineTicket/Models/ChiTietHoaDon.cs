using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class ChiTietHoaDon
{
    public int MaCthd { get; set; }

    public int? MaHd { get; set; }

    public int? MaVe { get; set; }

    public int? MaBn { get; set; }

    public int? SoLuong { get; set; }

    public virtual BapNuoc? MaBnNavigation { get; set; }

    public virtual HoaDon? MaHdNavigation { get; set; }

    public virtual Ve? MaVeNavigation { get; set; }
}
