using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class ChiTietHoaDon
{
    public int MaCthd { get; set; }
    public int? MaHd { get; set; }
    public int? MaVe { get; set; }  
    public int? MaBn { get; set; }  // DÒNG BẮP
    public int? SoLuong { get; set; } // >=1

    // NEW (rất nên có)
    public decimal? DonGia { get; set; } // decimal(18,2) — chốt giá tại thời điểm

    public virtual BapNuoc? MaBnNavigation { get; set; }
    public virtual HoaDon? MaHdNavigation { get; set; }
    public virtual Ve? MaVeNavigation { get; set; }
}