using System;
using System.Collections.Generic;

namespace CineTicket.Models;

public partial class HoaDon
{
    public int MaHd { get; set; }

    public DateTime? NgayLap { get; set; }

    public decimal? TongTien { get; set; }

    public string? HinhThucThanhToan { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
}
